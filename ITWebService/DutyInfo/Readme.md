# 说明
# 配置文件
DutyInfo目录
```
    linklist.json  #linklist.json文件中存放飞书个人链接
    Sites.json文件  #Sites.json文件存放站点信息
```
 站点对应目录下有两个json文件
```
    Rules.json      #排序规则定义文件
    DutyInfo.json   #班次信息定义
```
 ## DutyInfo.json文件介绍
```json
"schedule": {       
    "icon": "🌕",   
    "Place": "local",
    "Timeslot": "times"
  }
```
`schedule` 对应班次字段名，同一名字全局只可出现一次,填写时替换为具体的班次名称  
`icon` 表示图标  
`Place` 对应的值 local 表示值班地点 填写时替换为具体地点  
`Timeslot` 对应的 times 表示值班时间段  
举例
```json
"白班": {
    "icon": "🌕",
    "Place": "6楼IT服务间",
    "Timeslot": "08:30-17:30"
  }
```
## Rules.json文件介绍
```json
{
  "LocationInOnly": false,
  "Cycle": 4,
  "Dispute": {
    "白": {
      "1": "40F",
      "2": "15F"
    }
  },
  "MaskItem": [
    "白（初）",
    "休"
  ],
  "SortRules": {
    "Shift": [
      "早班",
      "常班",
      "白班",
      "中班",
      "晚班",
      "夜班"
    ],
    "Location": [
      "15F",
      "40F"
    ]
  }
}
```
`MaskItem` 表示屏蔽项目，将要屏蔽的班次填在这里即可。  
`SortRules` 表示排序规则，生效顺序按照`SortRules`子项排列顺序执行  
  `Shift`子项 表示对班表进行排序。
  `Location`子项 表示对值班地点进行排序。
# 文件同步
修改之前先运行syncpull.sh脚本，拉取服务端最新的数据，然后再进行数据修改。  
文件修改完成之后运行文件夹中的syncpush.sh脚本进行数据上传  
# 信息更新
每月涉及到的数据更新只有站点目录下的每月班表信息表格，样式为***duty.xlsx表格