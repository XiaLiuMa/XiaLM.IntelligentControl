using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseModule.Language
{
    public delegate void LangChanged();
    public enum LangType
    {
        English,
        Chinese
    }
    public class LangManager
    {
        private static LangManager instance = null;
        private LangType currentLang = LangType.English;

        public event LangChanged OnLangChanged;

        public static LangManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LangManager();
                }
                return instance;
            }
        }

        public LangType CurrentLang
        {
            get
            {
                return currentLang;
            }
            set
            {
                if (value != currentLang)
                {
                    currentLang = value;
                    EventTrigger.FireEvent(this, OnLangChanged, nameof(OnLangChanged));
                }
            }
        }

        private LangManager()
        {

        }

    }
}
