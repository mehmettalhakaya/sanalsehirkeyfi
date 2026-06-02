# Louvre Sanal Şehir Modeli Optimizasyon Çalışması

## Açıklama

Bu çalışma, Blender ortamında oluşturulan sanal şehir modelinin detay seviyesini artırmak ve performans açısından optimize etmek amacıyla hazırlanmıştır. Model üzerinde doku çözünürlüğü, poligon yoğunluğu ve malzeme kullanımı analiz edilerek optimizasyon işlemleri gerçekleştirilmiştir.  

- Mesh optimizasyonu
- Texture sıkıştırma işlemleri
- Collider düzenlemeleri

uygulanarak sahnenin performansı artırılmıştır.

---

# Model Hakkında

Projede kullanılan model, Paris'teki **Louvre Müzesi'nin iç ve dış mekanını** temsil etmektedir. Model hazır bir 3D asset olarak temin edilmiş ve Unity oyun motoruna entegre edilerek oynanabilir bir sanal gezi deneyimine dönüştürülmüştür.

---

# Dış Model

Louvre Müzesi’nin dış cephesi; tarihi saray yapısını, avlu düzenini ve ünlü cam piramidi kapsayan detaylı bir mimari modelden oluşmaktadır.

## İçerdiği Yapılar

- Tarihi dış cephe mimarisi
- Cam piramit yapısı
- Avlu ve çevresel düzenlemeler
- Cephe dokuları ve mimari detaylar

Model, Unity içerisinde **Outside_Museum** sahnesi altında konumlandırılmıştır. Oyuncu bu alandan müzeye giriş yapabilmektedir.

## Yapılan Optimizasyonlar

- Yüksek çözünürlüklü texture’lar sıkıştırıldı
- Uzak mesafede görünmeyen detaylar azaltıldı
- Gereksiz mesh yoğunluğu optimize edildi

---

# İç Model

Müze iç mekanı, toplam **28 ayrı mesh parçasından** oluşmaktadır.

## İç Mekan Öğeleri

- Duvarlar
- Tablolar
- Kapı çerçeveleri
- Zemin yapıları
- Tavan detayları

İç mekan modeli Unity içerisinde **LouvreInteriorOptimized** sahnesi altında yapılandırılmıştır.

---

# Fizik ve Collider Düzenlemeleri

Oyuncu etkileşimini iyileştirmek amacıyla çeşitli collider optimizasyonları yapılmıştır.

## Mesh Collider Kullanımı

**LouvreScene_30** dış duvar modeli üzerinde **Mesh Collider** kullanılmıştır. Böylece karmaşık geometriye sahip yüzeylerde oyuncunun model içerisinden geçmesi engellenmiştir.

## Box Collider Kullanımı

**LouvreScene_11** üzerinde bulunan tablo ve iç duvar bölmeleri için **Box Collider** tercih edilmiştir. Bu sayede karakter hareketi oda sınırları içerisinde tutulmuştur.

## Trigger Bölgeleri

Kapı geçişleri için özel trigger alanları oluşturulmuştur. Oyuncu bu bölgeler aracılığıyla sahneler arasında geçiş yapabilmektedir.

---

# Texture Optimizasyonu

İç mekana ait yüksek çözünürlüklü texture’lar optimize edilerek sahnenin render maliyeti azaltılmıştır.

## Yapılan İşlemler

- 4K texture’lar 2K ve 1K seviyelerine düşürüldü
- Görsel kalite korunmaya çalışıldı
- Bellek kullanımı azaltıldı
- FPS performansı iyileştirildi

---

# Tamamlanan İşler

- [x] Louvre iç mekan modeli temin edildi (hazır asset kullanıldı)
- [x] Unity'e import edildi ve sahneye yerleştirildi
- [x] Dış duvar için Mesh Collider eklendi 
- [x] Tablo ve iç duvar collider ayarları yapıldı 
- [x] Karakter geçiş ve sıkışma sorunları giderildi
- [x] Texture çözünürlükleri düşürülerek performans optimizasyonu sağlandı

---

# Sonuç

Gerçekleştirilen optimizasyon çalışmaları sonucunda Louvre sanal müze modeli daha akıcı ve performanslı bir hale getirilmiştir. Özellikle collider düzenlemeleri ve texture optimizasyonları sayesinde oyuncu deneyimi iyileştirilmiş, sahne render maliyeti azaltılmıştır.
