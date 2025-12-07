using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizes2.Models
{
    internal class TestData
    {
        public string Title { get; set; }
        public List<Question> Questions { get; set; } = new();
        public List<TestResult> Results { get; set; } = new();
    }
}
