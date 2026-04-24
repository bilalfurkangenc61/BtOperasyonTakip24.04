using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BtOperasyonTakip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IssueController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IssueController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("report")]
        public async Task<ActionResult<Issue>> ReportIssue([FromBody] CreateIssueDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Başlık zorunludur.");

            var issue = new Issue
            {
                Title = dto.Title,
                Description = dto.Description ?? "",
                Reporter = dto.Reporter ?? "Saha",
                Status = IssueStatus.Bekleme,
                CreatedAt = DateTime.UtcNow
            };

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetIssue), new { id = issue.Id }, issue);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Issue>> GetIssue(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound();
            return issue;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Issue>>> GetAllIssues()
        {
            return await _context.Issues.OrderByDescending(i => i.CreatedAt).ToListAsync();
        }
    }

    public class CreateIssueDto
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? Reporter { get; set; }
    }
}