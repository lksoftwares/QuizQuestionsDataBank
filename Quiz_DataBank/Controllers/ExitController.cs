using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Quiz_DataBank.Controllers
{
    [ApiController]
    public class ExitController : ControllerBase
    {
        [HttpGet]
        [Route("/exit")]
        public IActionResult ExitApplication()
        {
            try
            {

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c C:\\Windows\\System32\\TASKKILL.EXE /F /IM Quiz_DataBank.exe",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    process.WaitForExit();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode == 0)
                    {
                        return Ok(new { message = "Process terminated successfully.", output });
                    }
                    else
                    {
                        return BadRequest(new { message = "Failed to terminate process.", error });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }

        }
        }
}
