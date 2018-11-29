using System;

namespace DotNetNote.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
