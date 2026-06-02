# Louvre Sanal Müze Modeli Optimizasyon Çalışması

## Proje Açıklaması

Bu çalışma, Blender ortamında oluşturulan sanal şehir modelinin Unity oyun motoruna entegre edilerek performanslı ve oynanabilir bir sanal müze deneyimine dönüştürülmesi amacıyla hazırlanmıştır. Projede kullanılan Louvre modeli yüksek detay seviyesine sahip olduğundan dolayı sahne içerisinde render maliyetini artırmakta ve oyuncu hareketleri sırasında performans problemlerine neden olmaktadır. Bu sebeple model üzerinde kapsamlı optimizasyon çalışmaları gerçekleştirilmiştir.

Çalışma kapsamında modelin:

- Texture çözünürlüğü
- Mesh yapısı
- Collider sistemi
- Sahne yerleşimi

analiz edilerek performans odaklı iyileştirmeler yapılmıştır. Bunun yanında sahnede fizik hesaplamalarını azaltmak için collider yapıları yeniden düzenlenmiş ve yüksek çözünürlüklü dokular sıkıştırılarak GPU yükü azaltılmıştır.

---

# Kullanılan Teknolojiler

| Teknoloji | Kullanım Amacı |
|---|---|
| Blender | 3D model düzenleme ve mesh kontrolü |
| Unity | Oyun motoru ve sahne yönetimi |
| C# | Karakter kontrolü ve sahne geçiş sistemleri |
| Mesh Collider | Karmaşık yüzeylerde fizik çarpışması |
| Box Collider | Basit fizik sınırlandırmaları |

---

# Model Hakkında

Projede kullanılan model, Paris'te bulunan **Louvre Müzesi'nin** hem dış hem de iç mekanınının belirli kısımlarını temsil etmektedir. Modelin bir kısmı hazır bir 3D asset olarak temin edilmiş ve Unity ortamına aktarılmış, gerekli eklemeler yapılmıştır.

Modelin temel amacı kullanıcıya gerçek müze deneyimine yakın bir sanal gezi ortamı sunmaktır. Oyuncu müzenin dış alanında serbest şekilde dolaşabilmekte, ardından iç mekana geçiş yaparak farklı tabloları keşfedebilmektedir.

Model genel olarak iki ana bölümden oluşmaktadır:

1. Dış Mekan Modeli
2. İç Mekan Modeli

---

# Dış Mekan Modeli

Louvre Müzesi’nin dış modeli; tarihi saray yapısı, geniş avlu sistemi ve müzenin simgesi olan cam piramit yapısını kapsamaktadır.

## Dış Model İçeriği

Dış model içerisinde:

- Ana bina cephesi
- Cam piramit
- Avlu taş döşemeleri
- Sütun ve pencere detayları
- Dış duvar yapıları
- Giriş alanları
- Işıklandırma sistemleri

yer almaktadır.

Model Unity içerisinde **Outside_Museum** sahnesi altında yapılandırılmıştır. Oyuncu oyuna bu bölgede başlamakta ve müze girişine (cam piramit) yönlendirilmektedir.

---

## Dış Mekan Optimizasyonları

Dış model yüksek poligon sayısına sahip olduğundan dolayı çeşitli optimizasyon teknikleri uygulanmıştır.

### Yapılan İşlemler

- Görünmeyen yüzeyler sahneden kaldırıldı
- Texture sıkıştırma işlemleri uygulandı
- Uzak mesafelerde gereksiz detaylar azaltıldı
- Bazı objelerde collider sayısı düşürüldü

---

# İç Mekan Modeli

Yapı toplamda **28 farklı mesh parçasından** oluşmaktadır.

Bu parçalar:

- LouvreScene_05
- LouvreScene_06
- ...
- LouvreScene_32

şeklinde organize edilmiştir.

İç model Unity içerisinde **LouvreInteriorOptimized** sahnesi altında yapılandırılmıştır.

---

## İç Mekan Yapıları

İç model aşağıdaki mimari öğeleri içermektedir:

### Mimari Öğeler

- Duvar sistemleri
- Tavan yapıları
- Zemin kaplamaları
- Sergi tabloları
- Işıklandırma sistemleri

---

# Collider ve Fizik Sistemi

Oyuncunun sahne içerisinde gerçekçi şekilde hareket edebilmesi için çeşitli fizik ve çarpışma sistemleri uygulanmıştır.

---

## Mesh Collider Kullanımı

Dış duvar sistemi üzerinde bulunan karmaşık geometri nedeniyle standart Box Collider yapıları yeterli olmamıştır. Bu sebeple **LouvreScene_30** objesi üzerinde **Mesh Collider** kullanılmıştır.

### Sağlanan Avantajlar

- Oyuncunun duvar içinden geçmesi engellendi
- Karmaşık yüzeylerde daha doğru çarpışma hesaplandı
- Duvar köşelerinde oluşan fizik hataları azaltıldı

---

## Box Collider Kullanımı

İç mekan içerisindeki tablolar ve bazı düz duvar bölmeleri için performans açısından daha uygun olan **Box Collider** yapısı tercih edilmiştir.

Özellikle:

- LouvreScene_11
- İç bölme duvarları
- Tablo alanları

üzerinde Box Collider kullanılmıştır.

### Sağlanan Avantajlar

- Fizik hesaplama maliyeti düşürüldü
- Daha stabil karakter hareketi sağlandı
- Gereksiz mesh fizik hesaplamaları azaltıldı

---

# Texture ve Render Optimizasyonu

Projede kullanılan bazı texture dosyaları başlangıçta yüksek çözünürlüklü olduğu için GPU kullanımını artırmıştır. Bu durum özellikle düşük sistemlerde FPS düşüşlerine neden olmuştur.

Bu problemi çözmek amacıyla texture optimizasyonu uygulanmıştır.

---

## Yapılan Texture İşlemleri

- 4K çözünürlüklü texture’lar 2K ve 1K seviyelerine düşürüldü
- Gereksiz normal map kullanımları kaldırıldı
- Texture compression aktif edildi
- Tekrarlayan texture kullanımları optimize edildi

---

## Sağlanan Kazanımlar

Yapılan optimizasyonlar sonucunda:

- GPU yükü azaltıldı
- Bellek kullanımı düşürüldü
- Render süresi belli bir süreye kadar iyileştirildi
- FPS değerlerinde artış sağlandı
- Sahne geçiş süreleri kısaltıldı

---

# Tamamlanan Çalışmalar

- [x] Louvre iç mekan modeli temin edildi
- [x] Model Unity ortamına aktarıldı
- [x] Outside_Museum sahnesi oluşturuldu
- [x] LouvreInteriorOptimized sahnesi oluşturuldu
- [x] Mesh Collider sistemi uygulandı
- [x] Box Collider düzenlemeleri yapıldı
- [x] Trigger geçiş sistemi kuruldu
- [x] Karakter sıkışma problemleri giderildi
- [x] Texture optimizasyonları gerçekleştirildi
- [x] Sahne performansı iyileştirildi
