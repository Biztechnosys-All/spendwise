using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.DLL;
using System.ComponentModel.DataAnnotations;
using static Spendwise_WebApp.Pages.registerModel;

namespace Spendwise_WebApp.Pages
{
    [IgnoreAntiforgeryToken]
    public class ContactUsModel : PageModel
    {
        private readonly EmailSender _emailSender;
        public ContactUsModel(EmailSender emailSender)
        {
            _emailSender = emailSender;
        }


        [BindProperty]
        public ContactFormModel ContactForm { get; set; }
        public List<SelectListItem> DepartmentList { get; set; }

        public void OnGet()
        {
            PopulateDepartmentList();
        }

        //public async Task<IActionResult> OnPostSubmitForm()
        //{
        //    PopulateDepartmentList();
        //    if (!ModelState.IsValid)
        //    {
        //        return Page(); // Redisplay form with validation errors
        //    }

        //    var emailBody = $@"
        //    <table cellpadding='0' cellspacing='0' border='0' style='width: 100%; font-family: Arial, sans-serif; color: #333;'>
        //        <tr>
        //            <td>
        //                <p style='font-size: 16px; margin-bottom: 10px;'>Hello Admin,</p>
        //                <p style='font-size: 14px; margin-bottom: 20px;'>
        //                    You have received a new contact request. The details are below:
        //                </p>
        //                <p style='font-size: 14px; line-height: 1.6; margin: 0;'>
        //                    <strong>Name:</strong> {ContactForm.Name}<br/>
        //                    <strong>Email:</strong> {ContactForm.Email}<br/>
        //                    <strong>Department:</strong> {ContactForm.Department}<br/>
        //                    <strong>Question:</strong><br/>
        //                    {System.Net.WebUtility.HtmlEncode(ContactForm.Question)}
        //                </p>
        //                <p style='font-size: 14px; margin-top: 20px;'>
        //                    Please take the necessary action.
        //                </p>

        //                <p style='font-size: 14px;'>
        //                    Regards,<br/>
        //                    <strong>Your Website</strong>
        //                </p>
        //            </td>
        //        </tr>
        //    </table>";

        //    await _emailSender.SendEmailAsync("himanshu.b@biztechnosys.com", "New Contact Request", emailBody);

        //    TempData["SuccessMessage"] = "Your message has been submitted successfully!";
        //    return Page();
        //}

        public async Task<JsonResult> OnPostSubmitFormAsync()
        {
            PopulateDepartmentList();

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        key = x.Key,
                        errors = x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    })
                    .ToArray();

                return new JsonResult(new { success = false, errors });
            }

            var emailBody = $@"
                <p>Hello Admin,</p>
                <p>You have received a new contact request. The details are below:</p>
                <p><strong>Name:</strong> {ContactForm.Name}<br/>
                <strong>Email:</strong> {ContactForm.Email}<br/>
                <strong>Department:</strong> {ContactForm.Department}<br/>
                <strong>Question:</strong><br/>
                {System.Net.WebUtility.HtmlEncode(ContactForm.Question)}</p>
                <p>Please take the necessary action.</p>
                <p>Regards,<br/><strong>Spendwise Accountancy</strong></p>
            ";

            await _emailSender.SendEmailAsync("himanshu.b@biztechnosys.com", "New Contact Request", emailBody);

            return new JsonResult(new { success = true, message = "Your message has been submitted successfully!" });
        }


        private void PopulateDepartmentList()
        {
            DepartmentList = new List<SelectListItem>
            {
                new SelectListItem { Value = "General", Text = "General Enquiry" },
                new SelectListItem { Value = "ID", Text = "ID requirements" },
                new SelectListItem { Value = "RenewalsCancellations", Text = "Renewals/Cancellations" },
                new SelectListItem { Value = "Mail", Text = "Mail" },
                new SelectListItem { Value = "Refunds", Text = "Refund" },
                new SelectListItem { Value = "VATPAYE", Text = "VAT/PAYE" },
                new SelectListItem { Value = "Complaint", Text = "Complaint" }
            };
        }
    }

    public class ContactFormModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public string Department { get; set; }

        [Required(ErrorMessage = "Question is required.")]
        public string Question { get; set; }
    }

}
