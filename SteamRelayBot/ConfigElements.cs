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

		[ConfigurationProperty("semanticName", IsKey = false, IsRequired = true)]
		public string SemanticName
		{
			get { return (string)this ["semanticName"]; }
		}
    }

	class SteamUserElement : ConfigurationElement
	{
		[ConfigurationProperty("steamID", IsKey = true, IsRequired = true)]
		public string SteamID
		{
			get { return (string)this["steamID"]; }
		}

		[ConfigurationProperty("notes", IsKey = false, IsRequired = true)]
		public string SemanticName
		{
			get { return (string)this ["notes"]; }
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

	class SteamUserElementCollection : ConfigurationElementCollection
	{
		public SteamUserElement this[int index]
		{
			get
			{
				return (SteamUserElement)BaseGet(index);
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
			return new SteamUserElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((SteamUserElement)element).SteamID;
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

	class AdminGroupChatConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("steamIDs")]
		public GroupChatElementCollection SteamIDs
		{
			get { return (GroupChatElementCollection)this["steamIDs"]; }
		}
	}

	class BlacklistedUsersConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("steamIDs")]
		public SteamUserElementCollection SteamIDs
		{
			get { return (SteamUserElementCollection)this["steamIDs"]; }
		}
	}
}
