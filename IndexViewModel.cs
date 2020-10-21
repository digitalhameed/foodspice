using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models.ViewModel
{
    public class IndexViewModel
    {
        public IEnumerable<MenuItem> menuItem { get; set; }
        public IEnumerable<Category>  category { get; set; }
        public IEnumerable<Coupon>  coupon { get; set; }
    }

}
