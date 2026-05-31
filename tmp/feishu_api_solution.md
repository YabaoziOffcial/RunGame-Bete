通用信息
调用域名

https://api.holopix.cn
接口验签
本指南用于指导开发者对接需要签名验证的接口，实现请求合法性校验，包括防篡改、防重放攻击及权限控制。所有的API接口必须严格遵循以下规范。
必传请求头参数


参数名

类型

必填

描述

X-Access-Key

String

是

访问标识，需提前向服务端申请（对应服务端ApiUser账号）

X-Signature

String

是

签名结果，客户端根据签名规则生成

X-Timestamp

String

是

客户端请求时间戳（毫秒级，如1695888888888）

X-Nonce

String

是

随机字符串（建议 32 位以上，如 UUID），同一值在 5 分钟内不可重复使用
完整验签流程
开发者需确保请求符合以下校验逻辑，否则会被拦截：
参数完整性校验
必须包含上述 4 个请求头参数，缺失则返回缺少签名参数错误。
时间戳有效性校验
客户端时间戳与服务端当前时间差不得超过 5 分钟（300000 毫秒），否则返回请求已过期。
Nonce 防重放校验
服务端通过 Redis 记录已使用的 nonce，同一 nonce 在 5 分钟内重复使用会返回重复的请求。
账号有效性校验
X-Access-Key需在服务端已注册，否则返回账号不存在。
路由权限校验
请求的 URL 路径（如/api/xxx/xxx）必须包含在该账号允许的请求路径中，否则返回没有访问权限。
签名有效性校验
服务端会重新计算签名并与X-Signature比对，不一致则返回签名验证失败。

Python示例
Python
import hashlib
import hmac
import json
import time
from typing import Dict, Any


def generate_signature(timestamp: str, params: Dict[str, Any], secret_key: str) -> str:
"""
生成 HMAC-SHA256 签名

:param timestamp: 时间戳（毫秒）
:param params: 请求参数
:param secret_key: 密钥
:return: 签名字符串
"""
# 将字典转换为紧凑格式的JSON字符串，并确保浮点数1.0保持为1.0
json_str = json.dumps(
params,
separators=(',', ':'),
ensure_ascii=False  # 保持中文不转义
)
string_to_sign = timestamp + "="
if params:
string_to_sign += json_str
print("待签名字符串:", string_to_sign)
# 生成HMAC-SHA256签名
signature = hmac.new(
secret_key.encode('utf-8'),
string_to_sign.encode('utf-8'),
hashlib.sha256
).hexdigest()

print("生成验签:", signature)
return signature


# 示例调用入口
if __name__ == "__main__":
# 获取当前时间戳（毫秒级）
timestamp = str(int(time.time() * 1000))
print("当前时间戳：", timestamp)

# 构造请求 body.data 参数示例
body_data = {
"data": {
"aspectRatios": "1:1",
"characterPose": "https://pino-img.yingzhongshare.com/2025-08-06/1754468338890-tk6KKcnNIplBnF-MUtX4f.png",
"enablePerturb": "true",
"faceDetail": "true",
"hdFix": "true",
"hdScale": 2,
"imageReference": "https://pino-img.yingzhongshare.com/2025-08-06/1754445732517-wup6.png",
"modelDetailList": [
{
"modelId": 588,
"strength": 1.00
}
],
"negativePrompt": "yellow",
"perturb": 1,
"prompt": "1girl,长发,蓝眼,金发,连衣裙,猫耳,双马尾,全身,粉色连衣裙,拿着法杖1girl,长发,蓝眼,金发,连衣裙,猫耳,双马尾,全身,粉色连衣裙,拿着法杖",
"referenceMode": "standard",
"referenceWeight": 1,
"seed": -1
}
}
# 设置签名密钥
secret_key = "xxx"

# 调用签名方法生成签名
signature = generate_signature(timestamp, body_data, secret_key)
Java示例
Java
import cn.hutool.core.util.StrUtil;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;
import java.nio.charset.StandardCharsets;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;

public class SignTest {

    public static void main(String[] args) throws NoSuchAlgorithmException, InvalidKeyException, JsonProcessingException {
        String body = "{\"data\":{\"hdScale\":1.50,\"sourceImage\":\"xxx.png\"}}";
        String secretKey = "xxx";
        String signature = generateSignature(body, secretKey, String.valueOf(System.currentTimeMillis()));
        System.out.println("生成签名:" + signature);
    }

    /**
     * 生成 HMAC-SHA256 签名（与客户端逻辑一致）
     *
     * @param timestamp 当前时间长 （与请求头X-Timestamp 保持一致）
     * @param body      请求body.data的参数(json格式)
     * @param secretKey 密钥
     * @return 验签
     * @throws NoSuchAlgorithmException
     * @throws InvalidKeyException
     * @throws JsonProcessingException
     */
    public static String generateSignature(String body, String secretKey, String timestamp) throws NoSuchAlgorithmException, InvalidKeyException, JsonProcessingException {
        String json = "";
        if (StrUtil.isNotBlank(body)) {
            ObjectMapper objectMapper = new ObjectMapper();
            // 先解析为Java对象，再以紧凑格式输出
            Object jsonObject = objectMapper.readValue(body, Object.class);
            json = objectMapper.writeValueAsString(jsonObject);
        }
        String stringToSign = timestamp + "=" + json;
        String ALGORITHM = "HmacSHA256";
        Mac mac = Mac.getInstance(ALGORITHM);
        mac.init(new SecretKeySpec(secretKey.getBytes(StandardCharsets.UTF_8), ALGORITHM));
        byte[] signatureBytes = mac.doFinal(stringToSign.getBytes(StandardCharsets.UTF_8));
        StringBuilder result = new StringBuilder();
        for (byte b : signatureBytes) {
            result.append(String.format("%02x", b));
        }
        return result.toString();
    }
}
请求格式示例

curl --location --request POST 'http://localhost:8083/pino_holopix_api/api/model/modelPublishList?pars=1' \
--header 'X-Signature: eacc63b4b48b6786653f8893115e83d7b462dd4f903118e59f162589f53d65ea' \
--header 'X-Access-Key: 1950472678696468480' \
--header 'X-Timestamp: 1753868621229' \
--header 'X-Nonce: 14db2f8d-3f19-4fbe-8956-cfe87b5df107' \
--header 'Content-Type: application/json' \
--data-raw '{
    "data": {
        "xxx": 1,
        "xxx": 20
    }
}'
响应参数


字段

参数说明

类型

code

通用返回码

int

data

通用返回数据类型

Object

msg

错误信息

string

success

是否成功

bool
响应格式

{
    "msg": "成功",
    "code": 0,
    "data": {
        "xxx":"",
        "xxx"：""
    },
    "success": true
}
错误码


业务码

业务码定义

0

成功

10000

缺少签名参数（X-Access-Key/X-Signature/X-Timestamp/X-Nonce）

10001

请求已过期（时间戳无效）

10002

重复的请求（nonce已使用

10003

账号不存在

10004

没有访问权限

10005

签名验证失败（请求被篡改或密钥不一致）

-1

业务请求错误（根据业务接口返回对应错误信息）

