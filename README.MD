## 用途

开发时需要模拟数据

## 特性

对代码无侵入

## 规则

### 步骤1.获取本地伪造数据

通过`PathAndQueryString`，从数据库中返回数据

### 步骤2.通过域匹配规则，请求真实url

页面可能存在大量请求，可能部分请求已经存在，手动伪造那么多数据也麻烦
匹配规则：优先使用字符最多的匹配规则

例子

|域|目标|
|---|-----|
|http://api.sample1.com/|http://localhost:9876|
|http://api.sample2.com/auth/|http://localhost:8765|

|示例url|实际请求|
|---|-----|
|http://api.sample1.com/auth/xxx |http://localhost:9876/auth/xxx|
|http://api.sample2.com/auth/xxx|http://localhost:8765/xxx|
|http://api.sample2.com/xxx|404|


## 使用方法

1. 配置好端口，比如9000
2. 启动服务器，设置域，/ -> http://localhost:9555（开发环境api）
3. 如果前端项目不改请求url，则可以使用代理，代理到 http://localhost:9000【第1步提到的端口】，或者修改前端项目的url为，将apiurl改为 http://localhost:9000
4. 在模拟数据表中，设置需要模拟的url及数据

## 后续修改

- [ ] 对不同域使用不同模拟数据表：支持多个项目