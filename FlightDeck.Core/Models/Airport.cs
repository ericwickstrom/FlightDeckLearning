using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightDeck.Core.Models
{
    public record Airport(
    string IataCode,
    string Name,
    string City,
    string Country,
    string Region
    );
}
