using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
namespace WebSite.Models
{

    public class LoginModel
    {
        /// <summary> 
        /// 用户名 
        /// </summary> 
        [Display(Name = "登录名", Description = "4-20个字符")]
        [Required(ErrorMessage = "×")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "×")]
        public string UserName
        {
            get;
            set;
        }

        /// <summary> 
        /// 密码 
        /// </summary> 
        [Display(Name = "登录密码", Description = "6-20个字符")]
        [Required(ErrorMessage = "×")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "×")]
        [DataType(DataType.Password)]
        public string UserPwd
        {
            get;
            set;
        }

        /// <summary> 
        /// 是否保存COOKIE 
        /// </summary> 
        //[DisplayName("记住我")]
        public bool RememberMe
        {
            get;
            set;
        }
    }
}
