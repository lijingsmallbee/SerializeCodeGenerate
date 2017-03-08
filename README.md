# SerializeCodeGenerate
use for java script binding for unity to generate serialize code from google protobuffer file
这个工具是为java script binding的序列化提供的 虽然解决了java script binding使用protobuffer的问题，
但是感觉效率上还是堪忧 既然是业务系统这种非高频的包，其实是可以通过这种非压缩方式来进行网络交互的,工具还不完善，
目前只有c#版本 但是我已经把代码进行了分离，生成的部分基本是语言无关的，把语言相关的部分都封装 到了ByteArray内，
这样生成C++代码，java代码，只需要有对应的ByteArray实现即可 在unity中使用时，c#和js层主要就是通过ByteArray来
进行交互和参数传递，网络 socket也是基于ByteArray进行发送和接收后构建ByteArray交给js处理即可 因为使用了最少的
语言特性，转换为js之后不会有任何问题
