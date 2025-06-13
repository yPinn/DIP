using System.ComponentModel.DataAnnotations;

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "舊密碼")]
    public string OldPassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "新密碼")]
    public string NewPassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "兩次輸入的新密碼不一致")]
    [Display(Name = "確認新密碼")]
    public string ConfirmPassword { get; set; }
}
