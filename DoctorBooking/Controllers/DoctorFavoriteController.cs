using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.Controllers
{
    [Route("api/doctor_favourite")]
    [ApiController]
    [Authorize] // Required: endpoints read the user ID from the JWT token claims
    public class DoctorFavouriteController : ControllerBase
    {
        private readonly IDoctorFavoriteService service;

        public DoctorFavouriteController(IDoctorFavoriteService service)
        {
            this.service = service;
        }
        [HttpPost("add_doctor_favorite")]
        [ProducesResponseType(typeof(GeneicResult<FavoriteResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GeneicResult<FavoriteResponseDTO>>> Add_Favorite_Doctor([FromBody] CreateDoctorFavoriteDTO dTO )
        {
            var result = await service.Add_Favorite_Doctor(dTO.doctor_id);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("remove_doctor_favorite/{doctor_Id}")]
        [ProducesResponseType(typeof(GeneicResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GeneicResult<string>>> Remove_Doctor_From_favorites(long doctor_Id)
        {
            var result = await service.Remove_Favorite_Doctor(doctor_Id);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("read_favorites")]
        [ProducesResponseType(typeof(GeneicResult<List<FavoriteDoctorDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GeneicResult<List<FavoriteDoctorDTO>>>> Get_Favorites([FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = await service.GetFavorites(page, pageSize);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("doctor_exists_in_favorites/{doctor_id}")]
        [ProducesResponseType(typeof(GeneicResult<CheckFavoriteDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GeneicResult<CheckFavoriteDTO>>> CheckDoctorFavorite(long doctor_id)
        {
            var result = await service.check_favorite(doctor_id);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

    }
}