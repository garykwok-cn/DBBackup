# DBBackup
# mysql 自动备份和备份文件迁移
## 编译通过后在bin目录中创建setting.xml文件，用来设置数据源信息和迁移信息
### 目前文件名中支持定义以下关键字：
- 前一天：{predate}
- 当天：{date}
```
<?xml version="1.0" encoding="utf-8"?>
<setting>
  <backup>
    <dbServers>
      <server>
        <url>mysql数据库地址</url>
        <port>端口</port>
        <user>帐号</user>
        <pwd>密码</pwd>
        <databases>
          <db>
            <name>数据库名1</name>
            <bakFileName>数据库名-{date}.sql</bakFileName>
          </db>
          <db>
            <name>数据库名2</name>
            <bakFileName>数据库名-{date}.sql</bakFileName>
          </db>
        </databases>
        <folderName>{date}-文件夹其他部分</folderName>
        <savePath>保存路径（d:\backup\）</savePath>
        <compressName>{date}-压缩文件名其他部分.7z</compressName>
        <removeFolder>压缩完是否删除文件夹（1/0）</removeFolder>
      </server>
    </dbServers>
  </backup>
  <transferSet>
    <transfer>
      <sourcePath>备份文件保存目录（d:\backup\）</sourcePath>
      <destPath>目标文件目录（E:\backup\）</destPath>
      <files>需要拷贝迁移的文件（多个用分号【;】分隔，{predate}-120.7z;{predate}-130.7z）</files>
      <removeSource>迁移完是否源文件夹中的文件（1/0）</removeSource>
      <history>
        <removeHistory>删除历史记录（0/1）</removeHistory>
        <storeDays>文件保存天数（25）</storeDays>
      </history>
    </transfer>
  </transferSet>
</setting>
```
