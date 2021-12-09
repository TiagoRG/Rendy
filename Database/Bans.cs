using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class Bans
    {
        private readonly RendyContext _context;

        public Bans(RendyContext context)
        {
            _context = context;
        }
    }
}
