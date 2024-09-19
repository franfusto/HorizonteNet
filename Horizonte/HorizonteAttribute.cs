

namespace Horizonte
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HorizonteModule : Attribute
    {
        public string Modulename;

        public HorizonteModule(string name)
        {
            this.Modulename = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HConfigOption : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HorizonteCommand : Attribute
    {
        public string Key;
        public string Description;

        public HorizonteCommand(string key,string description = "" )
        {
            this.Key = key;
            this.Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HorizonteRole : Attribute
    {
        public string Role;

        public HorizonteRole(string role)
        {
            this.Role = role;
        }
    }

    
}