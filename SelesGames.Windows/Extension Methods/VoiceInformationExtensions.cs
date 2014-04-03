using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Media.SpeechSynthesis;

namespace SelesGames.Phone
{
    public static class VoiceInformationExtensions
    {
        public static IEnumerable<VoiceInformation> GetCultureFilteredVoices(this IEnumerable<VoiceInformation> voices, string nameHint = null)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (currentCulture == null)
                return voices;

            var regionCode = currentCulture.Name;
            if (string.IsNullOrEmpty(regionCode) || regionCode.Length < 2)
                return voices;

            IEnumerable<VoiceInformation> filtered = null;
            var language = regionCode.Substring(0,2);
          
            filtered = voices.Where(o => o.Language.Length > 1 && o.Language.Substring(0,2) == language);
            
            filtered = filtered.Any() ? filtered : voices;

            if (!string.IsNullOrEmpty(nameHint))
            {
                var hintedVoice = filtered.FirstOrDefault(o => o.DisplayName.IndexOf(nameHint, StringComparison.OrdinalIgnoreCase) > -1);
                if (hintedVoice != null)
                    filtered = new[] { hintedVoice }.Union(filtered);
            }

            return filtered;
        }
    }
}
