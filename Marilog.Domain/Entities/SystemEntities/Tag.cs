using Marilog.Domain.Common;


namespace Marilog.Domain.Entities.SystemEntities
{
    public class Tag : Entity
    {
        public string Name { get; private set; } = default!;
        public string Color { get; private set; } = "#1E88E5";
        public int StoredFileId { get; private set; }

        private Tag() { }

        public static Tag Create (string name, string color, int storedFileId)
        {
          return  new() 
          { 
              Name = name,
              Color = color,
              StoredFileId = storedFileId
          };
        }
            
    }
}
