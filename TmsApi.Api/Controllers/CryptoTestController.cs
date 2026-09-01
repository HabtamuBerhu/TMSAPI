// using Microsoft.AspNetCore.Mvc;
// using TmsApi.Infrastructure.Services;

// namespace TmsApi.Api.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class CryptoTestController : ControllerBase
// {
//     [HttpGet]
//     public IActionResult Test()
//     {
//         var service = new CryptoDemoService();

//         string password = "Password123!";

//         string hash1 = service.HashUserPassword(password);
//         string hash2 = service.HashUserPassword(password);

//         bool match1 = service.VerifyUserPassword(password, hash1);
//         bool match2 = service.VerifyUserPassword(password, hash2);

//         return Ok(new
//         {
//             hash1,
//             hash2,
//             hashesAreDifferent = hash1 != hash2,
//             match1,
//             match2
//         });
//     }
// }