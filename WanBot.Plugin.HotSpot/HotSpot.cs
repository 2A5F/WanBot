﻿using HtmlAgilityPack;
using SkiaSharp;
using System.Text;
using System.Text.Json;
using System.Web;
using WanBot.Api;
using WanBot.Api.Event;
using WanBot.Api.Message;
using WanBot.Api.Mirai;
using WanBot.Graphic;
using WanBot.Graphic.UI;
using WanBot.Graphic.UI.Layout;
using WanBot.Graphic.Util;
using WanBot.Plugin.Essential.Graphic;
using WanBot.Plugin.Essential.Permission;

namespace WanBot.Plugin.HotSpot
{
    public class HotSpot : WanBotPlugin
    {
        public override string PluginName => "HotSpot";

        public override string PluginAuthor => "WanNeng";

        public override string PluginDescription => "热点插件，可获取当前的微博热搜";

        public override Version PluginVersion => Version.Parse("1.0.0");

        private List<string> _searchCache = new();
        private string _hotspotCache = string.Empty;
        private DateTime _cacheTime = DateTime.MinValue;

        private UIRenderer _renderer = null!;
        public override void Start()
        {
            base.Start();

            _renderer =
                Application.PluginManager.GetPlugin<GraphicPlugin>()?.Renderer
                ?? throw new Exception("Failed to get renderer");
        }

        [Command("今日热点")]
        public async Task OnHotSpotCommand(MiraiBot bot, CommandEventArgs commandEvent)
        {
            if (!commandEvent.Sender.HasCommandPermission(this, "今日热点"))
                return;

            commandEvent.Blocked = true;

            var msgBuilder = new MessageBuilder();
            using var img = new MiraiImage(bot, await GetHotSpotAsync());
            msgBuilder.Image(img);
            await commandEvent.Sender.ReplyAsync(msgBuilder); 
        }

        public async Task<SKImage> GetHotSpotAsync()
        {
            var web = new HtmlWeb();
            var topicHtmlDoc = await web.LoadFromWebAsync("https://weibo.cn/pub/");
            var topics = topicHtmlDoc.DocumentNode.SelectNodes("/html/body//div[@class=\"c\"]");

            _searchCache.Clear();
            foreach (var topic in topics)
                _searchCache.Add(topic.InnerText);

            var containerId = HttpUtility.UrlEncode($"100103type=1&t=10&q={_searchCache[0]}", Encoding.UTF8);
            var url = $"https://m.weibo.cn/api/container/getIndex?containerid={containerId}&page_type=searchall";
            var hotHtmlDoc = await web.LoadFromWebAsync(url);
            var text = hotHtmlDoc.DocumentNode.InnerText;
            var searchResult = JsonSerializer.Deserialize<WeiboSearchResult>(text);

            var blog = searchResult!.data.cards.Where((card) => card.card_type == 9).FirstOrDefault();

            using var grid = new Grid();
            grid.Width = 800;
            var bg = new Rectangle();
            bg.Paint.Color = SKColors.LightGray;
            grid.Children.Add(bg);

            var horizontalLayout = new HorizontalLayout();
            grid.Children.Add(horizontalLayout);

            var verticalHelper = new VerticalHelper();
            verticalHelper
                .Box("微博热搜", SKColors.White, SKColors.DarkOrange, 32, 5);

            if (searchResult.data.cardlistInfo != null)
                verticalHelper = verticalHelper.Box($"【{searchResult.data.cardlistInfo.cardlist_title}】{searchResult.data.cardlistInfo.desc}", SKColors.White, SKColors.Black, 26, textAlignment: SKTextAlign.Left);
            verticalHelper
                .Box($"{blog!.mblog.user.screen_name} 表示：\n{blog.mblog.text.Replace("</span>", "").Replace("</a>", "")}", SKColors.White, SKColors.Black, 26, margin: 10, radius: 0, textAlignment: SKTextAlign.Left)
                .Width(800)
                .Space(10);

            SKImage? thumbnailPic = null;

            try
            {
                if (blog.mblog.thumbnail_pic != null)
                {
                    using var httpClient = new HttpClient();
                    using var imgStream = await httpClient.GetStreamAsync(blog.mblog.thumbnail_pic);
                    using var data = SKData.Create(imgStream);
                    thumbnailPic = SKImage.FromEncodedData(data);
                    verticalHelper.Add(new ImageBox
                    {
                        Image = thumbnailPic,
                        Width = thumbnailPic.Width,
                        Height = thumbnailPic.Height
                    });
                }
                verticalHelper.VerticalLayout.Margin = new Margin(0, 0, 0, 0);
                horizontalLayout.Children.Add(verticalHelper.VerticalLayout);

                verticalHelper.Box($"{_searchCache[1]}\n{_searchCache[2]}\n{_searchCache[3]}\n...", SKColors.White, SKColors.Black, 21, margin: 10, radius: 0, textAlignment: SKTextAlign.Left);

                return _renderer.Draw(grid);
            }
            finally
            {
                thumbnailPic?.Dispose();
            }
        }
    }
}