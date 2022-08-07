using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.ReconnectBan
{
    public static class Util
    {
        public static string Translate(string TranslationKey, params object[] Placeholders) =>
            ReconnectRestrictor.Inst.Translations.Instance.Translate(TranslationKey, Placeholders);
    }
}
