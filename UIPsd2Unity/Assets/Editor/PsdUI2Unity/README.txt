
PsdUI2Unity  版本更新日志

plan:
 1. 字体效果，下划线，描边，阴影
 2. 简化UISprite，UITexture使用，通过资源查找组件类型
 3. 编辑器可编辑图层，改名，保存Psd
 4. 新增条件约束，同子层命名不能相同
 5. 如何处理艺术字？ 嵌入BmFont，生成艺术库字体集


2017-3-13 v0.0.2
fixed:
1.修改ignore数据结构解析

add:
1.添加结点导出工具
2.添加Lua代码组件绑定
3.添加font关键字用于定义文本大小、描边及阴影
4.添加texture大贴图支持
5.添加UIDragScroll绑定
6.添加组件绑定的异常提示

modified
1.修改UIButton为EventListener
2.修改结点查询的逻辑顺序


2017-2-27 v0.0.1
1.添加Psd导出NGUI支持
2.添加Prefab通用模板支持，可自定义模板