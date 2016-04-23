using System.Configuration;

namespace SteamRelayBot
{
    class GroupChatElement : ConfigurationElement
    {
        [ConfigurationProperty("steamID", IsKey = true, IsRequired = true)]
        public string SteamID
        {
            get { return (string)this["steamID"]; }
        }
    }

    class GroupChatElementCollection : ConfigurationElementCollection
    {
        public GroupChatElement this[int index]
        {
            get
            {
                return (GroupChatElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new GroupChatElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((GroupChatElement)element).SteamID;
        }
    }

    class AutojoinGroupChatConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("steamIDs")]
        public GroupChatElementCollection SteamIDs
        {
            get { return (GroupChatElementCollection)this["steamIDs"]; }
        }
    }
}
