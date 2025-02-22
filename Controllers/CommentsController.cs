﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using RochaBlogs.Data;
using RochaBlogs.Models;

namespace RochaBlogs.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string slug, [Bind("PostId,Body")] Comment comment)
        {
           //if (ModelState.IsValid)
           //{
                
                comment.BlogUserId = _userManager.GetUserId(User);
                comment.Created = DateTime.UtcNow;
            // i added this
                comment.ModeratorId = _userManager.GetUserId(User);
                comment.ModeratedBody = _userManager.GetUserId(User);

                _context.Add(comment);
                await _context.SaveChangesAsync();

                comment = await _context.Comments.Include(c => c.Post).Where(c => c.Id == comment.Id).FirstOrDefaultAsync();
                
                if (comment == null)
                {
                    return NotFound();
                }

                return RedirectToAction("Details", "Posts", new { slug }, "commentSection");
           // }

         //   return RedirectToAction("Details", "Posts", new { slug }, "commentSection");
        }


        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Body")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var newComment = await _context.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == comment.Id);

                try
                {
                    newComment.Body = comment.Body;
                    newComment.Updated = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Posts", new { slug = newComment.Post.Slug }, "commentSection");
            }

            return RedirectToAction("Details", "Posts", new { slug = comment.Post.Slug }, "commentSection");
        }

        [Authorize(Roles = "Administrator, Moderator")]
        public async Task<IActionResult> Moderate(int id, [Bind("Id,Body,ModeratedBody,ModerationType")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

          //  if (ModelState.IsValid)
          //  {
                var newComment = await _context.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == comment.Id);
                
                try
                {
                    newComment.ModeratedBody = comment.ModeratedBody;
                    newComment.ModerationType = comment.ModerationType;
                    newComment.Moderated = DateTime.UtcNow;
                    newComment.ModeratorId = _userManager.GetUserId(User);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("Details", "Posts", new { slug = newComment.Post.Slug }, "commentSection");
           // }

          //  return RedirectToAction("Details", "Posts", new { slug = comment.Post.Slug }, "commentSection");
        }

        // POST: Comments/Unmoderate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Moderator")]
        public async Task<IActionResult> Unmoderate(int id, string slug)
        {
            if (_context.Comments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Comments'  is null.");
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment is not null)
            {
                comment.Moderated = null;
                comment.ModeratedBody = null;
                comment.ModeratorId = null;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Posts", new { slug }, "commentSection");
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Moderator")]
        public async Task<IActionResult> DeleteConfirmed(int id, string slug)
        {
            if (_context.Comments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Comments'  is null.");
            }
            var comment = await _context.Comments.FindAsync(id);
            if (comment is not null)
            {
                _context.Comments.Remove(comment);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Posts", new { slug }, "commentSection");
        }

        private bool CommentExists(int id)
        {
          return (_context.Comments?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
