using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace WebApi_171013.Controllers
{
    public class AutoController : ApiController
    {

        //測試api是否可連
        // GET api/auto
        public IEnumerable<string> Get()
        {
            return new string[] { "auto_value1", "auto_value2" };
        }

        //驗證回調URL
        public long Get(String msg_signature, String timestamp, String nonce, String echostr)
        {
            //企业微信后台开发者设置的token, corpID, EncodingAESKey
            string sToken = "5WQvoxc7HKzxSWKCc3O";
            string sCorpID = "wwb2491d1e47ba94f8";
            string sEncodingAESKey = "4CyeXxKsWzkYMxepDmdUHzNYHQoJ6QbAFPVN8OvUG4p";

            /*
			------------使用示例一：验证回调URL---------------
			*企业开启回调模式时，企业微信会向验证url发送一个get请求 
			假设点击验证时，企业收到类似请求：
			* GET /cgi-bin/wxpush?msg_signature=5c45ff5e21c57e6ad56bac8758b79b1d9ac89fd3&timestamp=1409659589&nonce=263014780&echostr=P9nAzCzyDtyTWESHep1vC5X9xho%2FqYX3Zpb4yKa9SKld1DsH3Iyt3tP3zNdtp%2B4RPcs8TgAE7OaBO%2BFZXvnaqQ%3D%3D 
			* HTTP/1.1 Host: qy.weixin.qq.com

			* 接收到该请求时，企业应			1.解析出Get请求的参数，包括消息体签名(msg_signature)，时间戳(timestamp)，随机数字串(nonce)以及企业微信推送过来的随机加密字符串(echostr),
			这一步注意作URL解码。
			2.验证消息体签名的正确性 
			3.解密出echostr原文，将原文当作Get请求的response，返回给企业微信
			第2，3步可以用企业微信提供的库函数VerifyURL来实现。
			*/

            Tencent.WXBizMsgCrypt wxcpt = new Tencent.WXBizMsgCrypt(sToken, sEncodingAESKey, sCorpID);

            string sVerifyMsgSig = msg_signature;
            string sVerifyTimeStamp = timestamp;
            string sVerifyNonce = nonce;
            string sVerifyEchoStr = echostr;
            int ret = 0;
            string sEchoStr = "";
            ret = wxcpt.VerifyURL(sVerifyMsgSig, sVerifyTimeStamp, sVerifyNonce, sVerifyEchoStr, ref sEchoStr);
            if (ret != 0)
            {
                return ret;
            }
            //ret==0表示验证成功，sEchoStr参数表示明文，用户需要将sEchoStr作为get请求的返回参数，返回给企业微信。
            return Convert.ToInt64(sEchoStr);
        }

        //接收消息並解密,處理後重新加密回傳微信
        [HttpPost]
        public String Post(String msg_signature, String timestamp, String nonce)
        {
            //企业微信后台开发者设置的token, corpID, EncodingAESKey
            string sToken = "5WQvoxc7HKzxSWKCc3O";
            string sCorpID = "wwb2491d1e47ba94f8";
            string sEncodingAESKey = "4CyeXxKsWzkYMxepDmdUHzNYHQoJ6QbAFPVN8OvUG4p";

            /*
            ------------使用示例二：对用户回复的消息解密---------------
			用户回复消息或者点击事件响应时，企业会收到回调消息，此消息是经过企业微信加密之后的密文以post形式发送给企业，密文格式请参考官方文档
			假设企业收到企业微信的回调消息如下：
			POST /cgi-bin/wxpush? msg_signature=477715d11cdb4164915debcba66cb864d751f3e6&timestamp=1409659813&nonce=1372623149 HTTP/1.1
			Host: qy.weixin.qq.com
			Content-Length: 613
			<xml>			<ToUserName><![CDATA[wx5823bf96d3bd56c7]]></ToUserName><Encrypt><![CDATA[RypEvHKD8QQKFhvQ6QleEB4J58tiPdvo+rtK1I9qca6aM/wvqnLSV5zEPeusUiX5L5X/0lWfrf0QADHHhGd3QczcdCUpj911L3vg3W/sYYvuJTs3TUUkSUXxaccAS0qhxchrRYt66wiSpGLYL42aM6A8dTT+6k4aSknmPj48kzJs8qLjvd4Xgpue06DOdnLxAUHzM6+kDZ+HMZfJYuR+LtwGc2hgf5gsijff0ekUNXZiqATP7PF5mZxZ3Izoun1s4zG4LUMnvw2r+KqCKIw+3IQH03v+BCA9nMELNqbSf6tiWSrXJB3LAVGUcallcrw8V2t9EL4EhzJWrQUax5wLVMNS0+rUPA3k22Ncx4XXZS9o0MBH27Bo6BpNelZpS+/uh9KsNlY6bHCmJU9p8g7m3fVKn28H3KDYA5Pl/T8Z1ptDAVe0lXdQ2YoyyH2uyPIGHBZZIs2pDBS8R07+qN+E7Q==]]></Encrypt>
			<AgentID><![CDATA[218]]></AgentID>
			</xml>

			企业收到post请求之后应该			1.解析出url上的参数，包括消息体签名(msg_signature)，时间戳(timestamp)以及随机数字串(nonce)
			2.验证消息体签名的正确性。
			3.将post请求的数据进行xml解析，并将<Encrypt>标签的内容进行解密，解密出来的明文即是用户回复消息的明文，明文格式请参考官方文档
			第2，3步可以用企业微信提供的库函数DecryptMsg来实现。
			*/

            StreamReader sr = new StreamReader(HttpContext.Current.Request.InputStream, Encoding.UTF8);
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(sr);
            sr.Close();
            sr.Dispose();

            //string sToUserName = doc.SelectSingleNode("xml").SelectSingleNode("ToUserName").InnerText;
            //string sAgentID = doc.SelectSingleNode("xml").SelectSingleNode("AgentID").InnerText;

            Tencent.WXBizMsgCrypt wxcpt = new Tencent.WXBizMsgCrypt(sToken, sEncodingAESKey, sCorpID);

            string sReqMsgSig = msg_signature;
            string sReqTimeStamp = timestamp;
            string sReqNonce = nonce;
            // Post请求的密文数据
            //string sReqData = "<xml><ToUserName><![CDATA[wx5823bf96d3bd56c7]]></ToUserName><Encrypt><![CDATA[RypEvHKD8QQKFhvQ6QleEB4J58tiPdvo+rtK1I9qca6aM/wvqnLSV5zEPeusUiX5L5X/0lWfrf0QADHHhGd3QczcdCUpj911L3vg3W/sYYvuJTs3TUUkSUXxaccAS0qhxchrRYt66wiSpGLYL42aM6A8dTT+6k4aSknmPj48kzJs8qLjvd4Xgpue06DOdnLxAUHzM6+kDZ+HMZfJYuR+LtwGc2hgf5gsijff0ekUNXZiqATP7PF5mZxZ3Izoun1s4zG4LUMnvw2r+KqCKIw+3IQH03v+BCA9nMELNqbSf6tiWSrXJB3LAVGUcallcrw8V2t9EL4EhzJWrQUax5wLVMNS0+rUPA3k22Ncx4XXZS9o0MBH27Bo6BpNelZpS+/uh9KsNlY6bHCmJU9p8g7m3fVKn28H3KDYA5Pl/T8Z1ptDAVe0lXdQ2YoyyH2uyPIGHBZZIs2pDBS8R07+qN+E7Q==]]></Encrypt><AgentID><![CDATA[218]]></AgentID></xml>";
            string sReqData = xdoc.InnerXml;
            string sError = "";
            string sMsg = "";  // 解析之后的明文
            string sRespData_p1 = "<xml><ToUserName><![CDATA[YuYuYi]]></ToUserName><FromUserName><![CDATA[wwb2491d1e47ba94f8]]></FromUserName><CreateTime>1348831860</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[";
            string sRespData_p3 = "]]></Content><MsgId>1234567890123456</MsgId><AgentID>1000002</AgentID></xml>";
            string sRespData_p2 = "";
            string sRespData = "";  // 需要发送的明文
            string sEncryptMsg = ""; //xml格式的密文
            int ret = 0;
            ret = wxcpt.DecryptMsg(sReqMsgSig, sReqTimeStamp, sReqNonce, sReqData, ref sMsg);
            if (ret != 0)
            {
                sRespData_p2 = "ERR: 解密失敗, ret: " + ret;
                sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                ret = wxcpt.EncryptMsg(sRespData, sReqTimeStamp, sReqNonce, ref sEncryptMsg);
                return sEncryptMsg;
            }
            // ret==0表示解密成功，sMsg表示解密之后的明文xml串
            // TODO: 对明文的处理
            string content = "";
            string event_type = "";
            string event_key = "";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sMsg);
            XmlNode root = doc.FirstChild;
            string msgtype = root["MsgType"].InnerText;
            switch (msgtype)
            {
                case "text":
                    content = root["Content"].InnerText;
                    sRespData_p2 = "您輸入: " + content;
                    sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                    break;
                case "event":
                    event_type = root["Event"].InnerText;
                    switch (event_type)
                    {
                        case "click":
                            event_key = root["EventKey"].InnerText;
                            switch (event_key)
                            {
                                case "menu_hit":
                                    sRespData_p2 = "您按了點擊測試鈕";
                                    sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                                    break;
                                default:
                                    sRespData_p2 = "您按了某個鈕";
                                    sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                                    break;
                            }
                            break;

                        case "scancode_push":
                            event_key = root["EventKey"].InnerText;
                            if (event_key == "menu_push")
                            {
                                //掃描後回傳xml
                                //< ScanCodeInfo >
                                //< ScanType >< ![CDATA[qrcode]] ></ ScanType >
                                //< ScanResult >< ![CDATA[1]] ></ ScanResult >
                                //</ ScanCodeInfo >
                                sRespData_p2 = root["ScanCodeInfo"].ChildNodes.Item(1).InnerText;
                                sRespData_p2 = "您的掃描值: " + sRespData_p2;
                                //sRespData_p2 = sRespData_p2.Replace("qrcode", "");
                                //if (string.IsNullOrEmpty(sRespData_p2))
                                //{
                                //    sRespData_p2 = "沒抓到掃描值";
                                //}
                                sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                            }
                            break;

                        case "scancode_waitmsg":
                            event_key = root["EventKey"].InnerText;
                            if (event_key == "menu_push2")
                            {
                                //掃描後回傳xml
                                //< ScanCodeInfo >
                                //< ScanType >< ![CDATA[qrcode]] ></ ScanType >
                                //< ScanResult >< ![CDATA[1]] ></ ScanResult >
                                //</ ScanCodeInfo >
                                sRespData_p2 = root["ScanCodeInfo"].ChildNodes.Item(1).InnerText;
                                sRespData_p2 = "您的掃描值: " + sRespData_p2;
                                //sRespData_p2 = sRespData_p2.Replace("qrcode", "");
                                //if (string.IsNullOrEmpty(sRespData_p2))
                                //{
                                //    sRespData_p2 = "沒抓到掃描值";
                                //}
                                sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                            }
                            break;

                        case "pic_sysphoto":
                            //拍照後回傳xml
                            //< ScanCodeInfo >
                            //< Count >1</ Count >
                            //< PicList >
                            //<item>
                            //<PicMd5Sum><![CDATA[1b5f7c23b5bf75682a53e7b6d163e185]]></PicMd5Sum>
                            //</item>
                            //</ PicList >
                            //</ ScanCodeInfo >
                            event_key = root["EventKey"].InnerText;
                            if (event_key == "menu_photo")
                            {
                                sRespData_p2 = root["ScanCodeInfo"].ChildNodes.Item(0).InnerText;
                                sRespData_p2 = "您傳送的拍照數量: " + sRespData_p2;
                                sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                            }
                            break;

                        case "location_select":
                            /*
                            定位後回傳xml
                            <SendLocationInfo>
                              <Location_X><![CDATA[23]]></Location_X>
                              <Location_Y><![CDATA[113]]></Location_Y>
                              <Scale><![CDATA[15]]></Scale>
                              <Label><![CDATA[ 广州市海珠区客村艺苑路 106号]]></Label>
                              <Poiname><![CDATA[]]></Poiname>
                            </SendLocationInfo>
                            */
                            event_key = root["EventKey"].InnerText;
                            if (event_key == "menu_gps")
                            {
                                string sLoc_X = root["SendLocationInfo"].ChildNodes.Item(0).InnerText;
                                string sLoc_Y = root["SendLocationInfo"].ChildNodes.Item(1).InnerText;
                                string sScale = root["SendLocationInfo"].ChildNodes.Item(2).InnerText;
                                string sLabel = root["SendLocationInfo"].ChildNodes.Item(3).InnerText;
                                string sPOI = root["SendLocationInfo"].ChildNodes.Item(4).InnerText;
                                sRespData_p2 = "座標X:" + sLoc_X
                                    + " 座標Y:" + sLoc_Y
                                    + " 精度:" + sScale
                                    + " 位置名稱:" + sLabel;
                                if (string.IsNullOrEmpty(sPOI) == false)
                                {
                                    sRespData_p2 = sRespData_p2 + " POI:" + sPOI;
                                }
                                sRespData_p2 = "您的GPS定位: " + sRespData_p2;
                                sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                            }
                            break;

                        case "LOCATION":
                            string sLatitude = root["Latitude"].InnerText;
                            string sLongitude = root["Longitude"].InnerText;
                            string sPrecision = root["Precision"].InnerText;
                            sRespData_p2 = "緯度:" + sLatitude
                                    + " 經度:" + sLongitude
                                    + " 精確度:" + sPrecision;
                            sRespData_p2 = "您的GPS定位: " + sRespData_p2;
                            sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                            break;

                        case "pic_photo_or_album":
                            /*
                            <SendPicsInfo>
                              <Count>1</Count>
                              <PicList>
                                <item>
                                  <PicMd5Sum><![CDATA[5a75aaca956d97be686719218f275c6b]]></PicMd5Sum>
                                </item>
                              </PicList>
                            </SendPicsInfo>
                            */
                            event_key = root["EventKey"].InnerText;
                            if (event_key == "menu_pic")
                            {
                                sRespData_p2 = root["SendPicsInfo"].ChildNodes.Item(0).InnerText;
                                sRespData_p2 = "您傳送的照片數量: " + sRespData_p2;
                                sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                            }
                            break;

                        case "pic_weixin":
                            /*
                            <SendPicsInfo>
                              <Count>1</Count>
                              <PicList>
                                <item>
                                  <PicMd5Sum><![CDATA[5a75aaca956d97be686719218f275c6b]]></PicMd5Sum>
                                </item>
                              </PicList>
                            </SendPicsInfo>
                            */
                            event_key = root["EventKey"].InnerText;
                            if (event_key == "menu_wx_pic")
                            {
                                sRespData_p2 = root["SendPicsInfo"].ChildNodes.Item(0).InnerText;
                                sRespData_p2 = "您傳送的微信照片數量: " + sRespData_p2;
                                sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                            }
                            break;

                        case "enter_agent":
                            sRespData_p2 = "您好,歡迎來到宏致電子";
                            sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                            break;

                        case "subscribe":  //關注
                            sRespData_p2 = "您好,已接收關注,感謝您關注宏致電子";
                            sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                            break;

                        case "unsubscribe":  //取消關注
                            sRespData_p2 = "您好,已取消關注,感謝您曾經關注宏致電子";
                            sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                            break;
                    }
                    break;
                default:
                    sRespData_p2 = "還未定義的MsgType";
                    sRespData = sRespData_p1 + sRespData_p2 + sRespData_p3;
                    break;
            }
            //content= sMsg.Replace(">","").Substring(0,48);
            //content = sMsg.Replace(">", "");

            /*
			------------使用示例三：企业回复用户消息的加密---------------
			企业被动回复用户的消息也需要进行加密，并且拼接成密文格式的xml串。
			假设企业需要回复用户的明文如下：
			<xml>
			<ToUserName><![CDATA[mycreate]]></ToUserName>
			<FromUserName><![CDATA[wx5823bf96d3bd56c7]]></FromUserName>
			<CreateTime>1348831860</CreateTime>
			<MsgType><![CDATA[text]]></MsgType>
			<Content><![CDATA[this is a test]]></Content>
			<MsgId>1234567890123456</MsgId>
			<AgentID>128</AgentID>
			</xml>

			为了将此段明文回复给用户，企业应：			1.自己生成时间时间戳(timestamp),随机数字串(nonce)以便生成消息体签名，也可以直接用从企业微信的post url上解析出的对应值。
			2.将明文加密得到密文。	3.用密文，步骤1生成的timestamp,nonce和企业在企业微信设定的token生成消息体签名。			4.将密文，消息体签名，时间戳，随机数字串拼接成xml格式的字符串，发送给企业。
			以上2，3，4步可以用企业微信提供的库函数EncryptMsg来实现。
			*/

            //content = xdoc.SelectSingleNode("xml").SelectSingleNode("ToUserName").InnerText;
            if (string.IsNullOrEmpty(content))
            {
                content = "vnull";
            }

            // 需要发送的明文
            ret = wxcpt.EncryptMsg(sRespData, sReqTimeStamp, sReqNonce, ref sEncryptMsg);
            if (ret != 0)
            {
                System.Console.WriteLine("ERR: EncryptMsg Fail, ret: " + ret);
                //return;
                sError = "ERR: 加密失敗, ret: " + ret;
            }

            return sEncryptMsg;

        }

    }
}
