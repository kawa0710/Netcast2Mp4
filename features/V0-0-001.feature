功能: 將podcast(本專案名為netcast)轉成mp4檔並提供使用者下載。

  場景: 轉mp4檔
    條件: 使用者進入轉檔網址且使用者輸入Rss feed
    當: Rss feed能取到資料(RssInfo)
    然後: 使用者選擇轉檔音訊(限3個)並排轉檔順序
    然後: 使用者輸入Email
    然後: 使用者上傳1個圖檔(gif/png/jpg)
    然後: 網頁提示
         # 下載網址
         # 查詢網址可用Email查詢
         # 轉檔完成寄信且內含下載網址
    然後: 送出
	
  測試資料:
    1. https://api.soundon.fm/v2/podcasts/666b9bb3-b53f-4623-ada3-4f311a141f93/feed.xml
    2. https://localhost:44327/GetRssInfo?url=https://api.soundon.fm/v2/podcasts/666b9bb3-b53f-4623-ada3-4f311a141f93/feed.xml

  場景: 下載mp4檔
    條件: 無
    當: 使用者進入下載網址且有轉好的音檔
    然後: 提供下載(一次下載一個,列檔案size)
    然後: 網頁提示清除時間
    當: 使用者進入下載網址且沒有轉好的音檔
    然後: 網頁提示沒有檔案
    然後: 網頁提示轉檔網址
	
