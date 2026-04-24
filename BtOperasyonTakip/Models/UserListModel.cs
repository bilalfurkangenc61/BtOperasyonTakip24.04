using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class UserListModel : PageModel
{
    private readonly AppDbContext _context;
    public List<User> Users { get; set; }

    public UserListModel(AppDbContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
        Users = _context.Users.ToList();
    }

    public IActionResult OnPostDelete(int id)
    {
        var user = _context.Users.Find(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
        return RedirectToPage();
    }
}