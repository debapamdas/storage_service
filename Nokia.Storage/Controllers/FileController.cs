using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nokia.Storage.FileStorage;

namespace Nokia.Storage.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileStorage _storage;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileStorage storage, ILogger<FileController> logger)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        // POST: api/file
        [HttpPost()]
        [ProducesResponseType(201, Type = typeof(StoredFileInfo))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> ImportFile([Required]IFormFile tarFile, CancellationToken cancellationToken)
        {
            try
            {
                if(tarFile.ContentType != Constants.TarBallContentType)
                {
                    _logger.LogError($"unsupported content type: {tarFile.ContentType}");
                    return BadRequest($"unsupported content type: {tarFile.ContentType}");
                }

                var storedFile = await _storage.StoreAsync(tarFile, cancellationToken);
                _logger.LogInformation("file imported sucessfully", storedFile.FileId);

                return CreatedAtAction(nameof(PullFile), storedFile);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating file");
                return StatusCode(500, "Unexpected error occured while creating new file.");
            }
        }
        // GET: api/file
        [HttpGet()]
        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(StoredFileInfo))]
        public async Task<IActionResult> PullFile([Required]string fileId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _storage.GetAsync(fileId, cancellationToken);
                if(result.Item1 != null || result.Item2 != null)
                {
                    return File(result.Item1, result.Item2.ContentType, result.Item2.FileName);
                }

                return NotFound(new StoredFileInfo { FileId = fileId });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling file");
                return StatusCode(500, "Unexpected error occured while pulling file.");
            }
        }

        // GET: api/file/list
        [HttpGet("list")]
        [ProducesResponseType(200, Type = typeof(PaginatedFileInfo))]
        public async Task<IActionResult> ListFiles(int pageSize, int pageNumber, CancellationToken cancellationToken)
        {
            try
            {
                if(pageSize <= 0)
                {
                    pageSize = 10;
                }
                if (pageNumber <= 0)
                {
                    pageNumber = 1;
                }
                var result = await _storage.ListAsync(pageSize, pageNumber, cancellationToken);
                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files");
                return StatusCode(500, "Unexpected error occured while listing files.");
            }
        }
    }
}
