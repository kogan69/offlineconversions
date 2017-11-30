using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.v201710;
using Google.Api.Ads.Common.Lib;

namespace OAuth
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            string conversionName = "TEST CONVERSION 1";
            // GCLID needs to be newer than 30 days.
            string gClId = "Cj0KCQiAjO_QBRC4ARIsAD2FsXMsi8DwS7vFdj2Fq6-BzM5enPjFfufUp8PH3Wznct03M0YLLeiBOsEaAgayEALw_wcB";
            //  The conversion time should be higher than the click time.
            string conversionTime = "20171130 100000";
            double conversionValue = double.Parse("213.11");

            UploadOfflineConvertion(new AdWordsUser(), conversionName, gClId, conversionTime, conversionValue);
            */
            CreateConversionTracker();

        }

        public static void CreateConversionTracker()
        {
            var user = new AdWordsUser();
            using (ConversionTrackerService conversionTrackerService =
                (ConversionTrackerService) user.GetService(AdWordsService.v201710.ConversionTrackerService))
            {
                List<ConversionTracker> conversionTrackers = new List<ConversionTracker>();

                AdWordsConversionTracker adWordsConversionTracker = new AdWordsConversionTracker();
                adWordsConversionTracker.name = "Earth to Mars Cruises Conversion";

                adWordsConversionTracker.category = ConversionTrackerCategory.DEFAULT;
                // Set optional fields.
                adWordsConversionTracker.status = ConversionTrackerStatus.ENABLED;
                adWordsConversionTracker.viewthroughLookbackWindow = 15;
                adWordsConversionTracker.defaultRevenueValue = 23.41;
                adWordsConversionTracker.alwaysUseDefaultRevenueValue = true;

                conversionTrackers.Add(adWordsConversionTracker);


                try
                {
                    // Create operations.
                    List<ConversionTrackerOperation> operations = new List<ConversionTrackerOperation>();
                    foreach (ConversionTracker conversionTracker in conversionTrackers)
                    {
                        operations.Add(new ConversionTrackerOperation()
                        {
                            @operator = Operator.ADD,
                            operand = conversionTracker
                        });
                    }

                    // Add conversion tracker.
                    ConversionTrackerReturnValue retval = conversionTrackerService.mutate(
                        operations.ToArray());

                    if (retval != null && retval.value != null)
                    {
                        foreach (ConversionTracker conversionTracker in retval.value)
                        {
                            if (conversionTracker is AdWordsConversionTracker)
                            {
                                AdWordsConversionTracker newAdWordsConversionTracker =
                                    (AdWordsConversionTracker) conversionTracker;
                                Console.WriteLine("Conversion with ID {0}, name '{1}', status '{2}', category " +
                                                  "'{3}' and snippet '{4}' was added.",
                                    newAdWordsConversionTracker.id, newAdWordsConversionTracker.name,
                                    newAdWordsConversionTracker.status, newAdWordsConversionTracker.category,
                                    newAdWordsConversionTracker.snippet);
                            }
                            else
                            {
                                Console.WriteLine("Conversion with ID {0}, name '{1}', status '{2}' and " +
                                                  "category '{3}' was added.", conversionTracker.id,
                                    conversionTracker.name,
                                    conversionTracker.status, conversionTracker.category);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No conversion trackers were added.");
                    }
                }
                catch (Exception e)
                {
                    throw new System.ApplicationException("Failed to add conversion trackers.", e);
                }
            }
        }

        public static void UploadOfflineConvertion(AdWordsUser user, String conversionName, String gClid, String conversionTime,
            double conversionValue)
        {
            using (var offlineConversionFeedService =
                (OfflineConversionFeedService) user.GetService(AdWordsService.v201710.OfflineConversionFeedService))
            {
                try
                {
                    // Associate offline conversions with the existing named conversion tracker. If
                    // this tracker was newly created, it may be a few hours before it can accept
                    // conversions.
                    OfflineConversionFeed feed = new OfflineConversionFeed();
                    feed.conversionName = conversionName;
                    feed.conversionTime = conversionTime;
                    feed.conversionValue = conversionValue;
                    feed.googleClickId = gClid;

                    OfflineConversionFeedOperation offlineConversionOperation =
                        new OfflineConversionFeedOperation();
                    offlineConversionOperation.@operator = Operator.ADD;
                    offlineConversionOperation.operand = feed;

                    OfflineConversionFeedReturnValue offlineConversionRetval =
                        offlineConversionFeedService.mutate(
                            new OfflineConversionFeedOperation[] {offlineConversionOperation});

                    OfflineConversionFeed newFeed = offlineConversionRetval.value[0];

                    Console.WriteLine("Uploaded offline conversion value of {0} for Google Click ID = " +
                                      "'{1}' to '{2}'.", newFeed.conversionValue, newFeed.googleClickId,
                        newFeed.conversionName);
                }
                catch (Exception e)
                {
                    throw new System.ApplicationException("Failed upload offline conversions.", e);
                }
            }
        }

        private static void DoAuth2Authorization(AdWordsUser user)
        {
            // Since we are using a console application, set the callback url to null.
            user.Config.OAuth2RedirectUri = null;
            AdsOAuthProviderForApplications oAuth2Provider =
                (user.OAuthProvider as AdsOAuthProviderForApplications);
            // Get the authorization url.
            string authorizationUrl = oAuth2Provider.GetAuthorizationUrl();
            Console.WriteLine("Open a fresh web browser and navigate to \n\n{0}\n\n. You will be " +
                              "prompted to login and then authorize this application to make calls to the " +
                              "AdWords API. Once approved, you will be presented with an authorization code.",
                authorizationUrl);

            // Accept the OAuth2 authorization code from the user.
            Console.Write("Enter the authorization code :");
            string authorizationCode = Console.ReadLine();


            // Fetch the access and refresh tokens.
            oAuth2Provider.FetchAccessAndRefreshTokens(authorizationCode);
        }



    }
}

