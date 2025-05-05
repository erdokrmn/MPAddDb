# MPAddDb

Bu proje, [https://www.millipiyangoonline.com/cekilis-sonuclari/super-loto](https://www.millipiyangoonline.com/cekilis-sonuclari/super-loto) adresinden Super Loto çekiliş verilerini çekip veritabanına kaydetmek amacıyla geliştirilmiştir. Python ve Selenium kullanılarak veriler dinamik olarak alınır ve MSSQL veritabanına işlenir.

## Özellikler

- Super Loto çekiliş verilerini otomatik olarak toplar.
- MSSQL veritabanına kayıt eder.
- Selenium ile tarayıcı otomasyonu sağlar.

## Gereksinimler

- Python 3.10+
- MSSQL Server
- Google Chrome (Sürüm: `chrome-win64`)
- ChromeDriver (Chrome sürümünüze uygun)
- Gerekli Python kütüphaneleri:
  - selenium
  - pyodbc
  - pandas
  - time
  - datetime

## Kurulum

1. Bu projeyi klonlayın:

   ```bash
   git clone https://github.com/erdokrmn/MPAddDb.git
   cd MPAddDb
