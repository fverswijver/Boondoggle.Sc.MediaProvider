using System.Web;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;

namespace Boondoggle.Sc.MediaProvider.Media
{
    /// <summary>
    /// Custom MediaRequestHandler that makes sure we go to the proper 404 when someone uploaded file media which can't be found
    /// </summary>
    public class MediaRequestHandler : Sitecore.Resources.Media.MediaRequestHandler
    {
        protected override bool DoProcessRequest(HttpContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            var mediaRequest = MediaManager.ParseMediaRequest(context.Request);
            if (mediaRequest == null)
                return false;

            bool bypassTo404 = false;
            var media = MediaManager.GetMedia(mediaRequest.MediaUri);
            if (media != null)
            {
                if (!Settings.Media.DisableFileMedia ||
                    (media.MediaData == null || media.MediaData.MediaItem == null || !media.MediaData.MediaItem.FileBased))
                {
                    return DoProcessRequest(context, mediaRequest, media);
                }
                else
                {
                    bypassTo404 = true;
                }
            }

            using (new SecurityDisabler())
            {
                media = MediaManager.GetMedia(mediaRequest.MediaUri);
            }

            string responseUrl;
            if (media == null || bypassTo404)
            {
                responseUrl = Settings.ItemNotFoundUrl;
            }
            else
            {
                Assert.IsNotNull(Context.Site, "site");
                responseUrl = Context.Site.LoginPage != string.Empty ? Context.Site.LoginPage : Settings.NoAccessUrl;
            }

            if (Settings.RequestErrors.UseServerSideRedirect)
                HttpContext.Current.Server.TransferRequest(responseUrl);
            else
                HttpContext.Current.Response.Redirect(responseUrl);

            return true;
        }
    }
}