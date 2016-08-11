using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonBag.State
{
    public class IResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
