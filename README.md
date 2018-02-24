# BiuBiu

protobuf消息发送工具。便于前后端在开发进度不一致的情况下，调试协议。


## Intro

*	common/proto：存放proto协议
*	common/tools/BiuBiu/generator.py：根据proto生成对于cs文件。将需要生成cs文件的proto，添加到`protoList`这个变量里面。`process_msg_file`方法对消息进行了简单的过滤。执行`ProtoToBiuBiu.bat`生成cs文件。
*	BiuBiu/Assets/Editor/BiuBiu/INet.cs：实现该接口，使用游戏内部的网络模块发送消息。可以参考`ProtoNet.cs`。


## Usage

点击Unity工具栏**BiuBiu/Start**。左侧选择消息，右侧设置参数。

*	Times：发送次数
*	Time Span：发送间隔（秒）


## Tips

*	暂不支持嵌套message，欢迎PR。
*	proto生成的cs存放到了Editor子目录。因为考虑到大部分项目主要使用Lua，proto直接生成pb，在Lua里面解析，在CSharp层不需要太多消息。可以根据项目需要修改`generator.py`里面的生成目录。


## Example

![biubiu_example](https://dn-ztgame.qbox.me/biubiu_example.png)