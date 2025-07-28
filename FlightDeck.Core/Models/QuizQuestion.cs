using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightDeck.Core.Models
{
    public record QuizQuestion(
        string Code,
        string CorrectAnswer,
        List<string> WrongAnswers,
        QuestionType Type
    );

    public enum QuestionType
    {
        CodeToAirport,    // Show "ATL" → guess "Atlanta"
        AirportToCode     // Show "Atlanta" → guess "ATL"
    }
}
