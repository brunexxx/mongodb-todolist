using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace M101DotNet.WebApp.Models.Home
{
    public class PostModel
    {
        public Todo Post { get; set; }

        public NewCommentModel NewComment { get; set; }
    }
}