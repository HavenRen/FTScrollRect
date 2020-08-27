# FTScrollRect
基于UGUI ScrollRect的高效滑动框  
  
cell有缓存，降低drawcall  
  
支持渐显效果，逐个实例化降低初始化的cup开销  
![img](https://github.com/HavenRen/FTScrollRect/blob/master/gif/common_scroll.gif)
  
不依赖LayoutGroup组件，减少rebuild
![img](https://github.com/HavenRen/FTScrollRect/blob/master/gif/grid_scroll.gif)
  
支持cell不同大小，可用在聊天框  
![img](https://github.com/HavenRen/FTScrollRect/blob/master/gif/mult_size_scroll.gif)
