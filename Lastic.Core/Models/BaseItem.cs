using Nest;

namespace Lastic.Core.Models
{
    public class BaseItem
    {
        [Number(NumberType.Integer)]
        public int Id { get; set; }
    }
}
