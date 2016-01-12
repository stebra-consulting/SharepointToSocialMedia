using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharepointToSocialMediaWeb
{
    public static class SPManager
    {
        //It is required to set this property in order to use SPManager
        public static HttpContextBase CurrentHttpContext { get; set; }

        public static ListItemCollection GetItemsFromGuid(string listGuid)
        {

            ListItemCollection items = null;

            var spContext = SharePointContextProvider.Current.GetSharePointContext(CurrentHttpContext);
            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    listGuid = listGuid.Replace("{", "").Replace("}", "");
                    Guid guid = new Guid(listGuid);

                    List list = clientContext.Web.Lists.GetById(guid);
                    clientContext.Load(list);
                    clientContext.ExecuteQuery();

                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = @"
                                                <View>
                                                    <Query>
                                                        <Where>
                                                            <IsNotNull>
                                                                <FieldRef Name='Title' />
                                                            </IsNotNull>
                                                        </Where>
                                                    </Query>
                                                </View>";
                    items = list.GetItems(camlQuery);

                    clientContext.Load(items);
                    clientContext.ExecuteQuery();
                }
            }
            return items;
        }
        public static ListItemCollection GetItemsFromListName(string listName)
        {
            ListItemCollection items = null;
            var spContext = SharePointContextProvider.Current.GetSharePointContext(CurrentHttpContext);
            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    List list = clientContext.Web.Lists.GetByTitle(listName);
                    clientContext.Load(list);
                    clientContext.ExecuteQuery();

                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = @"
                                        <View>
                                            <Query>
                                                <Where>
                                                    <IsNotNull>
                                                        <FieldRef Name='Title' />
                                                    </IsNotNull>
                                                </Where>
                                            </Query>
                                        </View>";
                    items = list.GetItems(camlQuery);

                    clientContext.Load(items);
                    clientContext.ExecuteQuery();

                }
            }
            return items;
        }

        public static System.IO.Stream ImportImage(string fileLeafRef)
        {
            System.IO.Stream stream = null;

            var spContext = SharePointContextProvider.Current.GetSharePointContext(CurrentHttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    List photoList = clientContext.Web.GetList("/sites/SD1/SiteAssets/");

                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = @"<View Scope='Recursive'>
                                            <Query>
                                                <Where>
                                                    <Eq>
                                                      <FieldRef Name='FileLeafRef'></FieldRef>
                                                      <Value Type='Text'>" + fileLeafRef + @"</Value>
                                                     </Eq>
                                                 </Where>
                                             </Query>
                                         </View>";
                    ListItemCollection photoCollection = photoList.GetItems(camlQuery);

                    clientContext.Load(photoCollection);

                    clientContext.ExecuteQuery();

                    ListItem item = photoCollection.ElementAt(0);

                    File file = item.File;

                    ClientResult<System.IO.Stream> data = file.OpenBinaryStream();

                    clientContext.Load(file);

                    clientContext.ExecuteQuery();

                    stream = data.Value;

                    //Wait for stream to get Executed by SharePoint
                    bool isLong = false;
                    while (isLong == false) { isLong = data.Value.Length is long; }
                    //Wait for stream to get Executed by SharePoint

                }
                return stream;
            }

        }
        public static void ToSocialMedia(string socialMedia)
        {
            ListItemCollection items = null;
            var spContext = SharePointContextProvider.Current.GetSharePointContext(CurrentHttpContext);
            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    List list = clientContext.Web.Lists.GetByTitle("Nyhetslista");
                    clientContext.Load(list);
                    clientContext.ExecuteQuery();

                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = @"
                                        <View>
                                            <Query>
                                                <Where>
                                                    <IsNotNull>
                                                       <FieldRef Name='Publicera' />
                                                    </IsNotNull>
                                                </Where>
                                            </Query>
                                        </View>";
                    items = list.GetItems(camlQuery);

                    clientContext.Load(items);
                    clientContext.ExecuteQuery();
                    foreach (ListItem item in items)
                    {

                        var publiceradArray = (string[])(item["Publicerad"]);
                        bool publicerad = publiceradArray.Contains("Publicerad till " + socialMedia);

                        var publiceraArray = (string[])(item["Publicera"]);
                        bool publicera = publiceraArray.Contains("Publicera till " + socialMedia);

                        if (publicerad == false && publicera == true)
                        {


                            //publicera till linkedin med bild, title, och länk

                            string title = item["Title"].ToString();

                            var id = item["ID"];

                            string link = "http://newsflashon.azurewebsites.net/Home/Nyheter/" + UrlManager.MakeURLFriendly(title);

                            string bodyAndArticle = item["Body"].ToString() + item["Article"].ToString();



                            string imgUrl = "";//firstImage(bodyAndArticle);



                            imgUrl = "https://stebrastash.blob.core.windows.net/photos/public_parrot.jpg";
                            bool PostBool = false;
                            try
                            {
                                if (socialMedia == "Facebook")
                                {
                                    PostBool = SocialMediaManager.postToFacebook(title, link, imgUrl);
                                }
                                if (socialMedia == "Linkedin")
                                {
                                    PostBool = SocialMediaManager.PostToLinkedIn(title, link, imgUrl);
                                }
                                

                            }
                            catch (Exception)
                            {

                                throw;
                            }
                            finally
                            {
#if DEBUG
                                PostBool = false;
#endif
                                if (PostBool)
                                {

                                    var newPub = new string[3];
                                    //change status of publicerad to publicerad till linkedin
                                    if (publiceradArray.Contains("Inte Publicerad"))
                                    {
                                        newPub = new[] { ("Publicerad till " + socialMedia) };
                                    }
                                    else
                                    {
                                        string mynewstring = "";
                                        foreach (string choice in publiceradArray)
                                        {
                                            mynewstring += choice + ",";
                                        }
                                        mynewstring = mynewstring + "Publicerad till " + socialMedia;
                                        newPub = mynewstring.Split(',');
                                    }

                                    item["Publicerad"] = newPub;
                                    item.Update();


                                }
                            }

                        }


                    }
                    clientContext.ExecuteQuery();


                }
            }


        }

        public static string firstImage(string bodyAndArticle)
        {
            string imageUrl = "none";
            if (imageUrl == "none")
            {
                string[] parts = bodyAndArticle.Split(' ');//split the string into pieces to find the src
                foreach (string part in parts)
                {
                    if (part.Contains("src"))//check for src
                    {
                        string[] partsOfSrc = part.Split('"');
                        imageUrl = partsOfSrc[1];
                        break;
                    }
                }
            }
            return imageUrl;
        }
    }
}