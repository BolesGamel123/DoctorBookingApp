using Application.Common.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace Tests.Common;

// Must be public for Moq/Castle.DynamicProxy to create proxies
public sealed record TestRequest(string Name) : IRequest<string>;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Should_PassThrough_WhenNoValidators()
    {
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var result = await behavior.Handle(
            new TestRequest("valid"),
            (ct) => Task.FromResult("success"),
            CancellationToken.None);

        result.Should().Be("success");
    }

    [Fact]
    public async Task Should_ThrowValidationException_WhenValidationFails()
    {
        var validator = new FailingValidator();
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });

        var act = () => behavior.Handle(
            new TestRequest(""),
            (ct) => Task.FromResult("should not reach"),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(2);
        exception.Which.Errors.Should().Contain("Name is required.");
        exception.Which.Errors.Should().Contain("Email is invalid.");
    }

    [Fact]
    public async Task Should_CallNext_WhenValidationPasses()
    {
        var validator = new PassingValidator();
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });
        var nextCalled = false;

        var result = await behavior.Handle(
            new TestRequest("valid"),
            (ct) => { nextCalled = true; return Task.FromResult("success"); },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("success");
    }

    [Fact]
    public async Task Should_DeduplicateErrors()
    {
        var validator = new DuplicateErrorValidator();
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });

        var act = () => behavior.Handle(
            new TestRequest(""), (ct) => Task.FromResult(""), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(1); // deduped
    }

    // ── Concrete validators (no Moq needed) ──────────────────
    private sealed class FailingValidator : AbstractValidator<TestRequest>
    {
        public FailingValidator()
        {
            RuleFor(x => x.Name).Must(_ => false).WithMessage("Name is required.");
            RuleFor(x => x.Name).Must(_ => false).WithMessage("Email is invalid.");
        }
    }

    private sealed class PassingValidator : AbstractValidator<TestRequest>
    {
        // No rules = always passes
    }

    private sealed class DuplicateErrorValidator : AbstractValidator<TestRequest>
    {
        public DuplicateErrorValidator()
        {
            RuleFor(x => x.Name).Must(_ => false).WithMessage("Required");
            RuleFor(x => x.Name).Must(_ => false).WithMessage("Required"); // duplicate
        }
    }
}
