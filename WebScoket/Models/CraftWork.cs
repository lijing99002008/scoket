using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace WebScoket.Models
{
    public class CraftWork
    {
        public int Id { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        /// <value>
        /// The item number.
        /// </value>
        public string ItemNum { get; set; }
        /// <summary>
        /// 文字描述
        /// </summary>
        /// <value>
        /// The craft describe.
        /// </value>
        public string CraftDescribe { get; set; }
        /// <summary>
        /// 工艺图
        /// </summary>
        /// <value>
        /// The craft image1.
        /// </value>
        public string CraftImage1 { get; set; }
        /// <summary>
        /// 工艺图
        /// </summary>
        /// <value>
        /// The craft image1.
        /// </value>
        public string CraftImage2 { get; set; }
        /// <summary>
        /// 工艺图
        /// </summary>
        /// <value>
        /// The craft image1.
        /// </value>
        public string CraftImage3 { get; set; }
    }
}