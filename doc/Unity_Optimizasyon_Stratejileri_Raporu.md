# Unity Optimizasyon Stratejileri Araştırması

**Proje:** Sanal Şehir Keşfi / Louvre Müzesi Keşfi  
**Konu:** Unity motorunda performans artırmaya yönelik optimizasyon stratejileri  
**Hazırlayan:** Mehmet Talha Kaya  
**Tarih:** 2026

---

## 1. Amaç

Bu raporun amacı, Unity ile geliştirilen sanal şehir / müze keşfi uygulamasında performansı artırmak için uygulanabilecek optimizasyon stratejilerini araştırmak ve proje içinde kullanılabilecek pratik bir yol haritası oluşturmaktır.

Uygulama 3B çevre, mimari modeller, ışıklandırma, gölgelendirme, kullanıcı etkileşimi ve sahne geçişleri içerdiği için performans; grafik ayarları, bellek kullanımı, script çalışma maliyeti ve sahne yönetimi gibi birden fazla başlık altında ele alınmalıdır.

---

## 2. Performans Analizi Yaklaşımı

Optimizasyona doğrudan ayar değiştirerek başlamak yerine önce performans darboğazı belirlenmelidir. Unity Profiler, CPU, GPU, bellek, render ve script maliyetlerini ayrı ayrı analiz etmeye yarar.

Önerilen analiz sırası:

1. Unity Profiler ile CPU ve GPU kullanımını ölçmek.
2. Frame Debugger ile sahnede kaç draw call oluştuğunu incelemek.
3. Memory Profiler ile texture, mesh, material ve runtime allocation maliyetlerini kontrol etmek.
4. Testleri sadece Editor içinde değil, hedef cihaz veya build üzerinde de yapmak.
5. Her optimizasyondan önce ve sonra FPS, frame time ve bellek kullanımını karşılaştırmak.

Ölçülmesi önerilen metrikler:

| Metrik | Açıklama |
| --- | --- |
| FPS | Saniyedeki kare sayısı |
| Frame Time | Bir karenin işlenme süresi |
| Draw Calls / Batches | GPU'ya gönderilen çizim komutları |
| SetPass Calls | Shader/material değişim maliyeti |
| GC Alloc | Garbage Collector'a yol açan bellek tahsisleri |
| Texture Memory | Texture dosyalarının GPU/bellek maliyeti |
| Shadow Maps | Gölge hesaplama maliyeti |

---

## 3. Grafik Optimizasyonu

### 3.1 Işıklandırma

Gerçek zamanlı ışıklar performans açısından maliyetlidir. Özellikle büyük sahnelerde çok sayıda real-time light kullanmak FPS düşüşüne neden olabilir.

Öneriler:

- Statik çevre objelerinde baked lighting kullanılmalıdır.
- Hareket etmeyen binalar, zemin, heykeller ve çevre objeleri `Static` olarak işaretlenmelidir.
- Gereksiz real-time light sayısı azaltılmalıdır.
- Işıkların range değerleri gereğinden büyük tutulmamalıdır.
- Sadece gerekli ışıklarda shadow aktif bırakılmalıdır.

Proje için uygulanabilir örnek:

Louvre dış mekanında sabit mimari objeler baked lighting ile aydınlatılabilir. Sokak lambaları veya dekoratif ışıklar düşük range değerleriyle sınırlandırılabilir.

### 3.2 Gölgelendirme

Gölge hesaplamaları özellikle büyük açık alanlarda performans maliyeti oluşturur.

Öneriler:

- Shadow Distance değeri düşürülmelidir.
- Gereksiz objelerde `Cast Shadows` kapatılmalıdır.
- Küçük dekoratif objelerin gölge oluşturması engellenmelidir.
- Gölge çözünürlüğü hedef cihaza göre ayarlanmalıdır.
- Çok uzaktaki objelerde gölge yerine baked shadow veya lightmap tercih edilmelidir.

### 3.3 Doku ve Materyal Optimizasyonu

Texture boyutları doğrudan bellek ve GPU performansını etkiler.

Öneriler:

- Gereksiz yüksek çözünürlüklü texture kullanılmamalıdır.
- Platforma uygun texture compression formatı seçilmelidir.
- Aynı görsel özelliklere sahip objelerde material sayısı azaltılmalıdır.
- Texture atlas kullanılarak draw call sayısı düşürülebilir.
- Normal map, metallic map ve emission map sadece gerçekten gerekiyorsa kullanılmalıdır.

Proje için uygulanabilir örnek:

Bina cepheleri, zemin kaplamaları ve tekrar eden mimari parçalar için ortak material/texture kullanımı artırılabilir.

### 3.4 LOD Kullanımı

Level of Detail, kameradan uzak objelerde daha düşük detaylı model kullanmayı sağlar.

Öneriler:

- Büyük mimari objeler için LOD Group eklenmelidir.
- Uzak binalar için düşük poligonlu mesh kullanılmalıdır.
- Çok uzaktaki küçük objeler tamamen gizlenebilir.
- LOD geçişleri görsel kaliteyi bozmayacak şekilde test edilmelidir.

Proje için uygulanabilir örnek:

Louvre binasının uzak cephe detayları, uzaktan bakıldığında daha düşük poligonlu LOD modellerle değiştirilebilir.

### 3.5 Occlusion Culling

Occlusion Culling, kameranın göremediği objelerin render edilmesini engeller. Kapalı veya yarı kapalı alanlarda önemli performans kazancı sağlayabilir.

Öneriler:

- Büyük, sabit objeler occluder olarak ayarlanmalıdır.
- Kamera tarafından görünmeyen bina arkaları ve iç mekan parçaları culling ile saklanmalıdır.
- Occlusion data bake edildikten sonra sahnede görsel test yapılmalıdır.
- Açık alanlarda etkisi sınırlı olabileceği için mutlaka profiler ile doğrulanmalıdır.

---

## 4. Script Optimizasyonu

### 4.1 Update Kullanımını Azaltma

Her frame çalışan `Update()` metodları gereksiz CPU tüketimine yol açabilir.

Öneriler:

- Sürekli kontrol gerektirmeyen işlemler event tabanlı yapılmalıdır.
- Belirli aralıklarla çalışması yeterli olan kontroller coroutine veya timer ile sınırlandırılmalıdır.
- `FindObjectOfType`, `GameObject.Find` gibi aramalar Update içinde kullanılmamalıdır.
- Sık kullanılan component referansları başlangıçta cache edilmelidir.

### 4.2 Object Pooling

Sürekli obje oluşturup yok etmek CPU ve bellek maliyeti oluşturur. Object Pooling ile objeler önceden oluşturulur ve tekrar kullanılır.

Kullanılabilecek alanlar:

- Partikül efektleri
- Etkileşim ikonları
- UI bildirimleri
- Tekrarlanan çevre objeleri
- Ses veya efekt tetikleyicileri

Proje için uygulanabilir örnek:

Bilgi panelleri, etkileşim göstergeleri veya kısa süreli efektler her seferinde oluşturulmak yerine havuzdan alınabilir.

### 4.3 Coroutine Kullanımı

Coroutine, zamana yayılan işlemleri tek frame içinde yığmadan çalıştırmak için yararlıdır. Ancak gereksiz ve kontrolsüz coroutine kullanımı da takip edilmesi zor performans sorunları oluşturabilir.

Öneriler:

- Sahne yükleme, bekleme ve zamanlanmış UI işlemlerinde coroutine kullanılabilir.
- Sonsuz coroutine döngüleri dikkatli yönetilmelidir.
- Coroutine başlatılan GameObject'in aktif kalmasına dikkat edilmelidir.
- Aynı anda gereksiz çok sayıda coroutine çalıştırılmamalıdır.

### 4.4 Garbage Collection Azaltma

Runtime sırasında sık sık bellek tahsisi yapılması GC spike oluşmasına neden olabilir. Bu da oyunda anlık takılmalar yaratır.

Öneriler:

- Update içinde yeni liste, string veya geçici obje oluşturulmamalıdır.
- String birleştirme işlemleri sık yapılıyorsa azaltılmalıdır.
- LINQ kullanımı performans kritik yerlerde sınırlandırılmalıdır.
- Geçici koleksiyonlar tekrar kullanılmalıdır.
- Object pooling uygulanmalıdır.

---

## 5. Bellek Yönetimi

Bellek optimizasyonu özellikle büyük 3B sahnelerde önemlidir. Gereksiz texture, mesh, audio ve material kullanımı hem yükleme süresini hem de runtime performansı etkiler.

Öneriler:

- Kullanılmayan assetler projeden temizlenmelidir.
- Texture boyutları hedef cihaza göre düşürülmelidir.
- Mesh collider yerine mümkün olduğunca basit collider kullanılmalıdır.
- Audio clip import ayarları kontrol edilmelidir.
- Büyük sahnelerde Addressables veya sahne bölme yaklaşımı değerlendirilebilir.

Proje için uygulanabilir örnek:

Dış müze sahnesi ile iç müze sahnesi ayrı tutulmalı, kullanılmayan iç mekan objeleri dış sahnede belleğe alınmamalıdır.

---

## 6. Mobil ve VR Cihazlar İçin Optimizasyon

Mobil VR cihazlar, masaüstü bilgisayarlara göre daha sınırlı CPU/GPU gücüne sahiptir. Bu nedenle VR hedefi varsa optimizasyon daha sıkı yapılmalıdır.

Öneriler:

- URP kullanılıyorsa mobil/XR odaklı ayarlar tercih edilmelidir.
- Post-processing efektleri minimumda tutulmalıdır.
- MSAA değeri hedef cihaza göre test edilmelidir.
- Gereksiz transparan materyaller azaltılmalıdır.
- Draw call sayısı düşürülmelidir.
- Stereo rendering maliyeti dikkate alınmalıdır.
- Hedef cihazda gerçek build testi yapılmalıdır.

VR için dikkat edilmesi gerekenler:

- FPS düşüşü kullanıcı konforunu doğrudan etkiler.
- Frame time sabit tutulmalıdır.
- Ani GC spike ve loading takılmaları engellenmelidir.
- UI elemanları okunabilir ve performans dostu olmalıdır.

---

## 7. Proje İçin Öncelikli Optimizasyon Planı

| Öncelik | İşlem | Beklenen Etki |
| --- | --- | --- |
| Yüksek | Unity Profiler ile başlangıç ölçümü almak | Darboğazı doğru belirlemek |
| Yüksek | Gereksiz real-time shadow ve light sayısını azaltmak | GPU maliyetini düşürmek |
| Yüksek | Büyük texture boyutlarını ve compression ayarlarını kontrol etmek | Bellek kullanımını azaltmak |
| Orta | LOD Group eklemek | Uzak objelerde render maliyetini düşürmek |
| Orta | Occlusion Culling test etmek | Görünmeyen objeleri render dışında bırakmak |
| Orta | Material sayısını azaltmak | Draw call azaltımı |
| Orta | Object pooling kullanmak | Instantiate/Destroy maliyetini azaltmak |
| Düşük | UI ve küçük efektlerde ekstra iyileştirmeler yapmak | Küçük ama temiz performans kazancı |

---

## 8. Test Senaryoları

Optimizasyonların etkisini ölçmek için aşağıdaki senaryolar kullanılabilir:

1. Ana menüden dış müze sahnesine geçiş süresi.
2. Dış müze sahnesinde kameranın Louvre binasına bakması.
3. Kamera hızlı döndürülürken FPS ve frame time ölçümü.
4. Yoğun ışık alan bölgede shadow maliyeti ölçümü.
5. Etkileşimli bilgi panelleri açılıp kapatılırken GC Alloc kontrolü.
6. İç mekan sahnesine geçişte bellek artışı kontrolü.
7. Build alınarak hedef cihazda FPS ölçümü.

---

## 9. Beklenen Sonuç

Bu optimizasyon stratejileri uygulandığında projenin daha stabil FPS değerlerine ulaşması, sahne geçişlerinin daha kontrollü olması, bellek kullanımının azalması ve özellikle düşük/orta seviye cihazlarda daha akıcı çalışması beklenir.

En önemli nokta, optimizasyonların tahmine göre değil ölçüme göre yapılmasıdır. Her değişiklikten sonra Profiler ile tekrar ölçüm alınmalı ve gerçekten performans artışı sağlayıp sağlamadığı kontrol edilmelidir.

---

## 10. Sonuç

Unity projelerinde performans optimizasyonu tek bir ayardan ibaret değildir. Grafik, script, bellek, sahne yönetimi ve hedef cihaz özellikleri birlikte değerlendirilmelidir. Sanal şehir keşfi gibi büyük 3B ortam içeren projelerde en etkili yaklaşım; önce ölçüm almak, sonra en yüksek maliyetli alanları sırayla iyileştirmektir.

Bu proje için ilk aşamada ışıklandırma, gölge, texture, LOD ve object pooling başlıklarına öncelik verilmesi önerilir. Daha sonra hedef cihaz testleriyle VR/mobil optimizasyonlar derinleştirilebilir.

---

## Kaynakça

- Unity Manual - Profiler: https://docs.unity.cn/Manual/Profiler.html
- Unity Manual - Profiler Window: https://docs.unity.cn/2023.3/Documentation/Manual/ProfilerWindow.html
- Unity Learn - Optimizing Graphics in Unity: https://learn.unity.com/tutorial/optimizing-graphics-in-unity
- Unity Learn - Introduction to Optimization in Unity: https://learn.unity.com/tutorial/introduction-to-optimization-in-unity
- Unity Learn - Object Pooling: https://learn.unity.com/tutorial/use-object-pooling-to-boost-performance-of-c-scripts-in-unity
- Unity Manual - Garbage Collection Best Practices: https://docs.unity.cn/Manual/performance-garbage-collection-best-practices.html
- Unity Manual - Occlusion Culling: https://docs.unity.cn/Manual/OcclusionCulling.html
- Unity Manual - Texture Compression Formats: https://docs.unity.cn/Manual/texture-compression-formats.html
- Unity Manual - XR Graphics: https://docs.unity.cn/Manual/xr-graphics.html
- Unity Manual - Optimize for Untethered XR Devices in URP: https://docs.unity.cn/6000.0/Documentation/Manual/urp/xr-untethered-device-optimization.html
