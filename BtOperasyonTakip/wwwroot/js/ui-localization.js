(function () {
    const storageKey = 'ui-language';
    const defaultLanguage = 'tr';
    const supportedLanguages = new Set(['tr', 'en']);
    const skipTags = new Set(['SCRIPT', 'STYLE', 'NOSCRIPT', 'CODE', 'PRE', 'TEXTAREA']);

    const exactTranslations = {
        tr: {
            'Operations Tracking': 'Operasyon Takip',
            'Operations Tracking System': 'Operasyon Takip Sistemi',
            'Home': 'Ana Sayfa',
            'Customers': 'Müşteriler',
            'Details': 'Detaylar',
            'Excel Import': 'Excel Aktarım',
            'Meeting Notes': 'Toplantı Notları',
            'Work Tracking': 'İş Takip',
            'Tickets': 'Ticketlar',
            'Issues': 'Hatalar',
            'Leave Tracking': 'İzin Takip',
            'Parameters': 'Parametreler',
            'User List': 'Kullanıcı Listesi',
            'User Management': 'Kullanıcı Yönetimi',
            'Actions': 'İşlemler',
            '+ Add New User': '+ Yeni Kullanıcı Ekle',
            'Sign In': 'Giriş',
            'Sign Out': 'Çıkış Yap',
            'Manage operations from a single hub.': 'Operasyon süreçlerini tek merkezden yönetin.',
            'Manage customers, tickets, issues, meeting notes, and work tracking flows in a faster and more organized way.': 'Müşteri, ticket, hata, toplantı notları ve iş takip akışlarını daha düzenli ve hızlı şekilde yönetin.',
            'Customer Management': 'Müşteri Yönetimi',
            'Ticket Tracking': 'Ticket Takibi',
            'Secure sign-in provides role-based screen routing.': 'Güvenli giriş ile rol bazlı ekranlara yönlendirme yapılır.',
            'Secure Sign-In': 'Güvenli Giriş',
            'Welcome': 'Hoş geldiniz',
            'Sign in with your account to continue': 'Devam etmek için hesabınızla giriş yapın',
            'Username': 'Kullanıcı Adı',
            'Enter your username': 'Kullanıcı adınızı girin',
            'Password': 'Şifre',
            'Enter your password': 'Şifrenizi girin',
            'Remember Me': 'Beni Hatırla',
            'Register': 'Kayıt Ol',
            'Create New Account': 'Yeni Hesap Oluştur',
            'Email': 'E-posta',
            'User Type': 'Kullanıcı Tipi',
            'Already have an account? Sign in': 'Zaten hesabın var mı? Giriş yap',
            'Welcome Page': 'Karşılama Sayfası',
            'You have successfully signed in to the IT Operations Tracking system.': 'BT Operasyon Takip sistemine başarıyla giriş yaptın.',
            'Enter Dashboard': 'Panele Giriş Yap',
            'Redirecting...': 'Yönlendiriliyor...',
            'Starting the system...': 'Sistemi başlatıyor...',
            'Establishing the database connection...': 'Veritabanı bağlantısı kuruluyor...',
            'Loading modules...': 'Modüller yükleniyor...',
            'Preparing the dashboard...': 'Dashboard hazırlanıyor...',
            'Monthly Filter': 'Aylık Filtre',
            'Apply': 'Uygula',
            'Total Customers': 'Toplam Müşteri',
            'Selected Month Customers': 'Seçili Ay Müşteri',
            'Added This Month': 'Aylık Eklenen',
            'Active Customers': 'Aktif Müşteri',
            'Selected Month Active Customers': 'Seçili Ay Aktif Müşteri',
            'Overall Portfolio': 'Genel Portföy',
            'Open Jira Items': 'Jira Açık',
            'Total Issues': 'Toplam Hata',
            'Customer Status': 'Müşteri Durumu',
            'Status distribution for the selected month': 'Seçili aydaki durum dağılımı',
            'Monthly Customer Growth': 'Aylık Müşteri Artışı',
            'New customer trend over the last 6 months': 'Son 6 ay yeni müşteri trendi',
            'Monthly New Record': 'Aylık Yeni Kayıt',
            'New record trend over the last 6 months': 'Son 6 ay yeni kayıt trendi',
            'New Records': 'Yeni Kayıtlar',
            'Jira Status': 'Jira Durumu',
            'Pending, active, and completed work items': 'Bekleyen, aktif ve tamamlanan işler',
            'Ticket Status': 'Ticket Durumu',
            'Awaiting approval, approved, and rejected items': 'Onay bekleyen, onaylanan ve reddedilenler',
            'System Tickets': 'Sistem Ticketları',
            'No Tickets Found': 'Ticket Bulunmuyor',
            'No tickets have been created yet.': 'Henüz hiç ticket oluşturulmamıştır.',
            'Ticket Search': 'Ticket Arama',
            'Filter by website, developer, email, or creator': 'Web sitesi, yazılımcı, mail veya oluşturan ile filtrele',
            'Search...': 'Arayın...',
            'File name / customer / description / added by...': 'Dosya adı / müşteri / açıklama / ekleyen...',
            'Clear': 'Temizle',
            'Search Company': 'Firma Ara',
            'Enter company name...': 'Firma adı yazın...',
            'Requested By': 'Kimden Geldi',
            'Open operation flow faster': 'İş akışını daha hızlı yönetin',
            'Manage person-based view, quick filtering, task status updates, and detail tracking on a single screen.': 'Kişi bazlı görünüm, hızlı filtreleme, görev durumu güncelleme ve detay takibini tek ekranda yönetin.',
            'View:': 'Görünüm:',
            'All People': 'Tüm Kişiler',
            'Search Customer': 'Müşteri Ara',
            'Company, contact, phone, technology...': 'Firma, yetkili, telefon, teknoloji...',
            'Excel Month (empty = All Months)': 'Excel Ay (boş = Tüm Aylar)',
            'Choose File': 'Dosya Seç',
            'No file selected': 'Dosya seçilmedi',
            'Excel Month': 'Excel Ay',
            'Full Excel': 'Tüm Excel',
            'New Customers Excel': 'Yeni Müşteriler Excel',
            'Filter': 'Filtrele',
            'Customer': 'Müşteri',
            'Authorized Person': 'Yetkili',
            'Status': 'Durum',
            'Status *': 'Durum *',
            'Description is required when changing status.': 'Durum değiştirirken açıklama zorunludur.',
            'Requester': 'Talep Sahibi',
            'Phone': 'Telefon',
            'Description': 'Açıklama',
            'Created Date': 'Kayıt Tarihi',
            'Status Date': 'Durum Tarihi',
            'Source': 'Kaynak',
            'Website URL': 'Site URL',
            'Technology': 'Teknoloji',
            'Company Name *': 'Firma Adı *',
            'Authorized Contact': 'Firma Yetkilisi',
            'Open Website': 'Siteyi Aç',
            'Total': 'Toplam',
            'Displayed': 'Gösterilen',
            'Page': 'Sayfa',
            'Previous': 'Önceki',
            'Next': 'Sonraki',
            'Search': 'Ara',
            'Search Section': 'Arama',
            'Filter by customer, creator, or note content.': 'Müşteri, ekleyen veya not içeriğine göre filtreleyin.',
            'All': 'Tümü',
            'Open Details': 'Detayları Aç',
            'Edit Customer': 'Müşteri Düzenle',
            'Select': 'Seçiniz',
            'Cancel': 'İptal',
            'Update': 'Güncelle',
            'Delete': 'Sil',
            'Date Range': 'Tarih Aralığı',
            'Add Work Item': 'İş Ekle',
            'Create a new record quickly with key information': 'Temel bilgilerle hızlı kayıt oluşturun',
            'Default status: Pending': 'Varsayılan durum: Beklemede',
            'Filters and view': 'Filtreler ve görünüm',
            'Manage person, search, and column view from a single panel': 'Kişi, arama ve kolon görünümünü tek panelden yönetin',
            'Person': 'Kişi',
            'Column Filter': 'Kolon Filtre',
            'Extra status columns': 'Ek durum kolonları',
            'Month': 'Ay',
            'Month:': 'Ay:',
            'May 2025': 'Mayıs 2025',
            'April 2025': 'Nisan 2025',
            'Visible work in selected month': 'Seçilen ayda görünen iş',
            'Select status': 'Durum seçiniz',
            'Selected columns': 'Seçilen kolonlar',
            'Work Type': 'İş Tipi',
            'Request Subject': 'Talep Konusu',
            'Assigned To': 'Takip Eden',
            'Search customer...': 'Müşteri ara...',
            'Search by typing...': 'Yazarak ara...',
            'Subject / Requester / Assignee / Status...': 'Konu / Açan / Takip Eden / Durum...',
            'This Page': 'Bu Sayfa',
            'Records awaiting action': 'Aksiyon bekleyen kayıt',
            'Ongoing work': 'Devam eden iş',
            'Closed work': 'Kapanmış iş',
            'In-progress work': 'Çalışılan işler',
            'Completed work': 'Sonuçlanan işler',
            'Records for this status': 'Bu duruma ait kayıtlar',
            'Rejected': 'Reddedildi',
            'Pending': 'Beklemede',
            'Active': 'Aktif',
            'Inactive': 'Pasif',
            'Completed': 'Tamamlandı',
            'Document Sent': 'Döküman İletildi',
            'Awaiting Approval': 'Onay Bekleyen',
            'Approved': 'Onaylanan',
            'Open': 'Açık',
            'Closed': 'Kapalı',
            'On Hold': 'Askıda',
            'Other': 'Diğer',
            'Issue': 'Hata',
            'Issue List': 'Hata Listesi',
            'New': 'Yeni',
            'No records found.': 'Kayıt yok.',
            'No records yet.': 'Henüz kayıt yok.',
            'No customers found for the selected criteria.': 'Aradığınız kriterlere uygun müşteri bulunamadı.',
            'Excel (Grouped by JiraId)': 'Excel (JiraId Gruplu)',
            'Excel (All Months)': 'Excel (Tüm Aylar)',
            'Development / Training List': 'Geliştirme / Eğitim Listesi',
            'Download Excel': 'Excel İndir',
            'Skip to main content': 'Ana içeriğe geç',
            'Error': 'Hata',
            'An error occurred while processing your request.': 'İsteğiniz işlenirken bir hata oluştu.',
            'Request ID:': 'İstek ID:',
            'Development Mode': 'Geliştirme Modu',
            'Home Page': 'Ana Sayfa',
            'Learn about building Web apps with ASP.NET Core.': 'ASP.NET Core ile web uygulamaları geliştirme hakkında bilgi edinin.',
            'Issue Management': 'Hatalar Yönetimi',
            'Download Excel File': 'Excel\'e İndir',
            'Add New Issue': 'Yeni Hata Ekle',
            'Search issue name, description, or customer...': 'Hata adı, açıklama veya müşteri ara...',
            'All Statuses': 'Tüm Durumlar',
            'All Categories': 'Tüm Kategoriler',
            'Software': 'Yazılım',
            'Hardware': 'Donanım',
            'Network': 'Ağ',
            'Database': 'Veritabanı',
            'Category': 'Kategori',
            'Assigned User': 'Atanan Kullanıcı',
            'Date': 'Tarih',
            'Leave Tracker': 'İzin Takip',
            'Filters': 'Filtreler',
            'Quickly filter by company, status, or requester.': 'Firma, durum veya talep eden kişiye göre hızlı filtreleme yapın.',
            'Requester': 'Talep Eden',
            'Filter by status': 'Duruma göre filtrele',
            'Filter by requester': 'Talep edene göre filtrele',
            'No tickets yet.': 'Henüz ticket bulunmamaktadır.',
            'Ticket List': 'Ticket Listesi',
            'Details, status, and workflow information are displayed in a single table.': 'Detay, durum ve süreç bilgileri tek tabloda gösterilir.',
            'Assigned Operation': 'Atanan Operasyon',
            'Compliance Decision Maker': 'Uyum Kararı Veren',
            'Contact': 'İrtibat',
            'Action': 'İşlem',
            'Yes': 'Evet',
            'No': 'Hayır',
            'Operation 1 Approval Pending': 'Operasyon 1 Onay Bekleniyor',
            'Operation 2 Approval Pending': 'Operasyon 2 Onay Bekleniyor',
            'Compliance Approval Pending': 'Uyum Onay Bekleniyor',
            'Customer Recorded': 'Musteri Kaydedildi',
            'Field Go-Live Pending': 'Saha Canli Bekleniyor',
            'Missing Documents Pending': 'Eksik Evrak Bekleniyor',
            'Meeting Notes': 'Toplantı Notları',
            'Parameter Management': 'Parametre Yönetimi',
            'Manage system types and parameter values centrally': 'Sistem içindeki tür ve parametre değerlerini merkezi olarak yönetin',
            'Excel Upload': 'Excel Aktarım',
            'Upload an Excel file, save its data to the database, and process matching customer details.': 'Excel yükleyin, veriler veritabanına kaydedilsin ve eşleşen müşterilerin detaylarına işlensin.',
            'Matched': 'Eşleşen',
            'Unmatched': 'Eşleşmeyen',
            'Conversation Details': 'Görüşme Detayları',
            'You can open details by selecting from customer cards.': 'Müşteri kartlarından seçim yaparak detayları açabilirsiniz.',
            'Customer details': 'Müşteri detayları',
            'Total:': 'Toplam:',
            'Pending:': 'Beklemede:',
            'Active:': 'Aktif:',
            'Document Sent:': 'Döküman İletildi:',
            'Customer #': 'Müşteri #',
            'Record:': 'Kayıt:',
            'Latest Detail': 'Son Detay',
            'Added Latest Comment': 'Son Yorumu Ekleyen',
            'Site': 'Site',
            'Latest Comment': 'Son Yorum',
            'Latest Work / Contact': 'Son İş / Görüşülen',
            'Selected Customer:': 'Seçilen Müşteri:',
            'User Management': 'Kullanıcı Yönetimi',
            'Actions': 'İşlemler',
            'Delete': 'Sil',
            'Total Records': 'Toplam Kayıt',
            'Total Types': 'Toplam Tür',
            'New Parameter': 'Yeni Parametre',
            'New Type': 'Yeni Tür',
            'Add Type': 'Tür Ekle',
            'Add New Type': 'Yeni Tür Ekle',
            'Status Management': 'Durum Yönetimi',
            'Parameter List': 'Parametre Listesi',
            'Parameter Name': 'Parametre Adı',
            'Parameter Type': 'Parametre Türü',
            'Type': 'Tür',
            'Order': 'Sıra',
            'Move Up': 'Yukarı',
            'Move Down': 'Aşağı',
            'Visible in Work Tracking columns': 'İş Takip kolonu olarak görünür',
            'Configure the order of columns and status visibility for the Work Tracking screen here.': 'İş Takip ekranındaki kolon sırasını ve durum görünümünü buradan yönetin',
            'Add a new parameter value to an existing type.': 'Mevcut bir türe yeni parametre değeri ekleyin',
            'Define a new type for parameter groups.': 'Parametre grupları için yeni tür tanımlayın',
            'Meeting Notes': 'Toplantı Notları',
            'Meeting Type': 'Toplantı Türü',
            'Work Tracking column': 'İş Takip kolonu',
            'Back to Work Tracking': 'İş Takip\'e Dön',
            'Excluded development and training work items are listed here.': 'Excel\'e dahil edilmeyen geliştirme ve eğitim işleri burada listelenir.',
            'No record found.': 'Kayıt bulunamadı.',
            'No record found': 'Kayıt bulunamadı',
            'New Parameter Value': 'Yeni Parametre Değeri',
            'Record No:': 'Kayıt No:',
            'Issue Name': 'Hata Adı',
            'Created By': 'Oluşturan',
            'Company Website': 'Müşteri Web Sitesi',
            'Developer Name': 'Yazılımcı',
            'Meeting Title': 'Toplantı Başlığı',
            'Meeting Notes': 'Toplantı Notları',
            'Meeting Type': 'Toplantı Türü',
            'Action Owner': 'Aksiyon Sahibi',
            'Target Date': 'Hedef Tarihi',
            'Participants': 'Katılımcı',
            'Conversation Date': 'Görüşme Tarihi',
            'Date Time': 'Tarih Saat',
            'Business / Contact': 'İş / Görüşülen',
            'Added By': 'Ekleyen',
            'Excel Transfer': 'Excel Aktarım',
            'Upload Excel': 'Excel Yükle',
            'Import starts if one of these columns exists: Customer/Company, Date, Work-Contact, Description, Added By.': 'Şu sütunlardan biri varsa aktarım yapılır: Müşteri/Firma, Tarih, İş-Görüşülen, Açıklama, Ekleyen.',
            'Matched Customers': 'Eşleşen Müşteriler',
            'Unmatched Customers': 'Eşleşmeyen Müşteriler',
            'Record deleted.': 'Kayıt silindi.',
            'Task added.': 'Görev eklendi.',
            'Status updated.': 'Durum güncellendi.',
            'Assigned user updated.': 'Takip eden güncellendi.',
            'Comment added.': 'Yorum eklendi.',
            'Comment cannot be empty.': 'Yorum boş olamaz.',
            'Invalid status update.': 'Geçersiz durum güncellemesi.',
            'Invalid work type selected.': 'Geçersiz iş tipi seçildi.',
            'Invalid record.': 'Geçersiz kayıt.',
            'Task not found.': 'Görev bulunamadı.',
            'Jira ID and Request Subject are required.': 'Jira ID ve Talep Konusu zorunludur.',
            'Selected customer could not be found.': 'Seçilen müşteri bulunamadı.',
            'Request Subject is required.': 'Talep Konusu zorunludur.',
            'Document status cannot be updated for this customer.': 'Bu müşteri için döküman durumu güncellenemez.',
            'Company name is required.': 'Firma adı zorunludur.',
            'Please select a valid status.': 'Geçerli bir durum seçiniz.',
            'Customer not found.': 'Müşteri bulunamadı.',
            'Document status updated.': 'Döküman durumu güncellendi.',
            'Customer added successfully!': 'Müşteri başarıyla eklendi!',
            'Customer updated successfully!': 'Müşteri başarıyla güncellendi!',
            'No records were found to import from Excel.': 'Excel içinde aktarılacak kayıt bulunamadı.',
            'Please select an Excel file.': 'Lütfen bir Excel dosyası seçiniz.',
            'Only .xlsx files can be uploaded.': 'Sadece .xlsx dosyaları yüklenebilir.',
            'Note content updated': 'Not içeriği güncellendi',
            'Created successfully.': 'Başarıyla oluşturuldu.',
            'Updated successfully.': 'Başarıyla güncellendi.',
            'Deleted successfully.': 'Başarıyla silindi.',
            'Issue': 'Hata',
            'Assigned': 'Atanan',
            'Detail': 'Detay',
            'Download to Excel': 'Excel\'e İndir',
            'Export to Excel': 'Excel\'e Aktar',
            'Leave allowance defined': 'Tanımlı izin hakkı',
            'Approved leave used': 'Onaylanmış kullanılan izin',
            'Remaining leave': 'Kalan izin',
            'day': 'gün',
            'Update Leave Request': 'İzin Talebini Güncelle',
            'New Leave Request': 'Yeni İzin Talebi',
            'Every saved change is sent back to admin approval.': 'Kaydedilen her değişiklik admin onayına yeniden düşer.',
            'Start Date': 'Başlangıç Tarihi',
            'End Date': 'Bitiş Tarihi',
            'Description / Reason': 'Açıklama / Mazeret',
            'Update and Send for Approval': 'Güncelle ve Onaya Gönder',
            'Create Request': 'Talep Oluştur',
            'User Leave Balances': 'Kullanıcı İzin Hakları',
            'Only operation users are listed.': 'Sadece operasyon kullanıcıları listelenir.',
            'User': 'Kullanıcı',
            'Total Allowance': 'Toplam Hak',
            'Used': 'Kullanılan',
            'Remaining': 'Kalan',
            'New Allowance Definition': 'Yeni Hak Tanımı',
            'No operation user found.': 'Operasyon kullanıcısı bulunamadı.',
            'Save': 'Kaydet',
            'All Leave Requests': 'Tüm İzin Talepleri',
            'My Leave Requests': 'İzin Taleplerim',
            'Edited records are moved back to pending.': 'Düzenlenen kayıtlar tekrar beklemeye alınır.',
            'Request Owner': 'Talep Sahibi',
            'Day': 'Gün',
            'Admin Note': 'Admin Notu',
            'Request Date': 'Talep Tarihi',
            'No leave request exists yet.': 'Henüz izin talebi bulunmuyor.',
            'Edit': 'Düzenle',
            'Approval note or rejection reason...': 'Onay notu veya red mazereti...',
            'Approve': 'Onayla',
            'Reject': 'Reddet',
            'Manage operation leave requests and define leave balances.': 'Operasyon izin taleplerini yönetin ve izin hakkı tanımlayın.',
            'Create or update your leave request and track the admin decision.': 'İzin talebinizi oluşturun, güncelleyin ve admin kararını takip edin.',
            'Total Parameter': 'Toplam Parametre',
            'No parameter is defined yet for the Status type.': 'Henüz Durum türünde parametre tanımlı değil.',
            'Defined types and parameter values': 'Tanımlı türler ve parametre değerleri',
            'Name': 'Ad',
            'The values of': 'değerleri parametrelerden çekilir.',
            'New Ticket Create': 'Yeni Ticket Oluştur',
            'New Ticket Oluştur': 'Yeni Ticket Oluştur',
            'Enter customer, payment, and technical details completely through a single form.': 'Müşteri, ödeme ve teknik bilgileri tek form üzerinden eksiksiz girin.',
            'Process starts: Operation 1': 'Süreç başlangıcı: Operasyon 1',
            'Customer type is required': 'Müşteri tipi zorunlu',
            'Ticket Information': 'Ticket Bilgileri',
            'Fill in the required fields and create the record.': 'Zorunlu alanları doldurup kaydı oluşturun.',
            'General Information': 'Genel Bilgiler',
            'Company Name': 'Firma Adı',
            'What Will the Customer Use?': 'Müşteri Ne Kullanacak?',
            'Who Developed the Website?': 'Web Sitesi Kim Tarafından Yazıldı?',
            'What Payment Methods Will Be Used?': 'Hangi Ödeme Yöntemleri Kullanılacak?',
            'Developer First Name': 'Yazılımcı Adı',
            'Developer Last Name': 'Yazılımcı Soyadı',
            'Email Address': 'Mail Adresi',
            'Contact Number': 'İrtibat Numarası',
            'Process Summary': 'Süreç Özeti',
            'After submission, the ticket directly moves to the Operation 1 approval step. Completing all required fields accelerates the process.': 'Kayıt sonrası ticket doğrudan Operasyon 1 onay adımına düşer. Gerekli alanları eksiksiz doldurmanız süreci hızlandırır.',
            'Things to Watch': 'Dikkat Edilecekler',
            'If there is already an open ticket for the same company, website, or email, the record is blocked.': 'Aynı firma, site veya mail için açık ticket varsa kayıt engellenir.',
            'Company, website, and contact details must be entered completely.': 'Firma, web sitesi ve iletişim bilgileri eksiksiz girilmelidir.',
            'Payment and website selections must be marked correctly.': 'Ödeme ve web sitesi seçimleri doğru işaretlenmelidir.',
            'Actions': 'Aksiyonlar',
            'Go to the Submit Button Below': 'Aşağıdaki Gönder Butonuna Git',
            'Back to List': 'Listeye Dön',
            'Send Ticket': 'Ticket Gönder',
            'At least one topic must be entered.': 'En az bir konu girmelisiniz.',
            'No records to display in this column.': 'Bu kolonda gösterilecek kayıt yok.',
            'Do you want to delete this record?': 'Bu kaydı silmek istiyor musunuz?',
            'Total (DB):': 'Toplam (DB):',
            'records · Page': 'kayıt · Sayfa',
            'Meeting record created.': 'Toplantı kaydı oluşturuldu.',
            'Record validation failed.': 'Kayıt doğrulanamadı.',
            'Empty status': 'Durum boş olamaz.',
            'Task not found': 'Görev bulunamadı.',
            'Status updated': 'Durum güncellendi.',
            'Error:': 'Hata:',
            'Invalid model': 'Geçersiz kayıt.',
            'Record not found': 'Kayıt bulunamadı.',
            'Yet': 'Henüz',
            'The up / down buttons define the column order.': 'Yukarı / aşağı butonları kolon sırasını belirler.',
            'No parameter is defined for the Status type yet.': 'Henüz Durum türünde parametre tanımlı değil.',
            'Customer Meeting Notes': 'Müşteri Toplantı Notları',
            'Create, review, and export meeting records.': 'Toplantı kayıtlarını oluşturun, inceleyin ve dışa aktarın.',
            'Type Options': 'Tür Seçenekleri',
            'Filter by customer, creator, or note content.': 'Müşteri, ekleyen veya not içeriğine göre filtreleyin.',
            'Search (Customer / Creator / Note)': 'Ara (Müşteri / Ekleyen / Not)',
            'New Meeting Record': 'Yeni Toplantı Kaydı',
            'Save meeting information and subject/target dates from a single form.': 'Toplantı bilgilerini ve konu/hedef tarihlerini tek formdan kaydedin.',
            'Meeting Title *': 'Toplantı Başlığı *',
            'Location *': 'Konum *',
            'Date *': 'Tarih *',
            'Time *': 'Saat *',
            'Prepared By *': 'Hazırlayan *',
            'Participants *': 'Katılımcılar *',
            'Topics and Target Dates *': 'Konular ve Hedef Tarihleri *',
            'Add New Topic': 'Yeni Konu Ekle',
            'Meeting Note': 'Toplantı Notu',
            'Meeting Minutes': 'Toplantı Tutanağı',
            'Name': 'İsim',
            'Meeting Title:': 'Toplantı Başlığı:',
            'Location:': 'Konum:',
            'Time:': 'Saat:',
            'Prepared By:': 'Hazırlayan:',
            'Participants:': 'Katılımcılar:',
            'Excel File (.xlsx)': 'Excel Dosyası (.xlsx)',
            'If a matching customer is found, the related record is also added to the Details page.': 'Eşleşen müşteri bulunursa ilgili kayıt Detaylar sayfasına da eklenir.',
            'Upload and Save': 'Yükle ve Kaydet',
            'Imported Records': 'İçe Aktarılan Kayıtlar',
            'You can view the Excel data uploaded on this page separately.': 'Bu sayfada yüklediğiniz Excel verilerini ayrı olarak görüntüleyebilirsiniz.',
            'Upload': 'Yükleme',
            'File': 'Dosya',
            'Row': 'Satır',
            'Uploader': 'Yükleyen',
            'Matched': 'Eşleşti',
            'Not Matched': 'Eşleşmedi',
            'Go to Details': 'Detaya Git',
            'No imported records yet.': 'Henüz aktarılmış kayıt yok.',
            'Username': 'Kullanıcı Adı',
            'Email Address': 'E-posta',
            'Meeting Minutes': 'Toplantı Tutanağı',
            'Meeting Title': 'Toplantı Başlığı',
            'Location': 'Konum',
            'Topic': 'Konu',
            'Target Date': 'Hedef Tarihi',
            'Prepared By': 'Hazırlayan',
            'Action Owner': 'Aksiyon Sahibi',
            'Please select an Excel file.': 'Lütfen bir Excel dosyası seçiniz.',
            'Excel could not be read:': 'Excel okunamadı:',
            'records were imported.': 'kayıt içe aktarıldı.',
            'records were added to details.': 'kayıt detaylara eklendi.',
            'Ticket created. Operation 1 approval is pending.': 'Ticket oluşturuldu. Operasyon 1 onayı bekleniyor.',
            'Invalid ticket ID.': 'Geçersiz ticket ID.',
            'Ticket not found.': 'Ticket bulunamadı.',
            'You cannot perform this action because this ticket is not assigned to you.': 'Bu ticket size atanmadığı için işlem yapamazsınız.',
            'Operation 1 decision saved. Compliance approval is pending.': 'Operasyon 1 kararı kaydedildi. Uyum onayı bekleniyor.',
            'Operation 2 decision saved.': 'Operasyon 2 kararı kaydedildi.',
            'Integration completed. Ticket was moved back to Field Go-Live Pending status.': 'Entegrasyon tamamlandı. Ticket tekrar Saha Canli Bekleniyor durumuna alındı.',
            'Go-live recorded. Customer record was created or updated.': 'Canlı açılış kaydedildi. Müşteri kaydı oluşturuldu veya güncellendi.',
            'Ticket rejected.': 'Ticket reddedildi.',
            'Missing document completed. Ticket was sent back to compliance approval.': 'Eksik evrak tamamlandı. Ticket yeniden uyum onayına gönderildi.',
            'Request is invalid.': 'Geçersiz istek.',
            'Change reason is required.': 'Değişiklik nedeni zorunlu.',
            'Change reason must be at most 500 characters.': 'Değişiklik nedeni maksimum 500 karakter olmalıdır.',
            'The selected user is not in the operation role or could not be found.': 'Seçilen kullanıcı operasyon rolünde değil veya bulunamadı.',
            'The selected status is not valid.': 'Seçilen durum geçerli değil.',
            'No assignee or status change was made.': 'Kişi veya durum değişikliği yapmadınız.',
            'Assignee assignment updated.': 'Kişi ataması güncellendi.',
            'Assignee and status updated.': 'Kişi ve durum güncellendi.',
            'An open ticket already exists for this company name.': 'Bu firma adı ile açık bir ticket zaten var.',
            'An open ticket already exists for this website.': 'Bu web sitesi ile açık bir ticket zaten var.',
            'An open ticket already exists for this email address.': 'Bu mail adresi ile açık bir ticket zaten var.',
            'Customer type must be either Corporate or Individual.': 'Müşteri tipi \u0027Kurumsal\u0027 veya \u0027Bireysel\u0027 olmalıdır.',
            'The user named test could not be found for corporate ticket assignment. The user must have the Operation role and UserName value test.': 'Kurumsal ticket ataması için \u0027test\u0027 kullanıcısı bulunamadı. Kullanıcı Operasyon rolünde olmalı ve UserName değeri \u0027test\u0027 olmalıdır.',
            'Only the field user who opened the ticket can perform this action.': 'Sadece ticket\u0027ı açan saha kullanıcısı bu işlemi yapabilir.',
            'Description is required.': 'Açıklama zorunlu.',
            'Assignee updated.': 'Kişi ataması güncellendi.',
            'Assignee and status updated. ({0} → {1})': 'Kişi ve durum güncellendi. ({0} → {1})',
            'Status updated. ({0} → {1})': 'Durum güncellendi. ({0} → {1})',
            'Integration action is not valid at this stage. Status:': 'Ticket bu aşamada entegrasyon tamamlama işlemine uygun değil. Durum:',
            'Operation 1 action is not valid at this stage. Status:': 'Ticket bu aşamada Operasyon 1 kararına uygun değil. Durum:',
            'Operation 2 action is not valid at this stage. Status:': 'Ticket bu aşamada Operasyon 2 kararına uygun değil. Durum:',
            'Go-live action is not valid at this stage. Status:': 'Ticket bu aşamada canlı açılışa uygun değil. Durum:',
            'Missing document completion is not valid at this stage. Status:': 'Ticket bu aşamada eksik evrak tamamlama işlemine uygun değil. Durum:',
            'Customer Website': 'Müşteri Web Sitesi',
            'Developer': 'Yazılımcı',
            'Issue Resolution': 'Hata Çözüm',
            'Development': 'Geliştirme',
            'Training': 'Eğitim',
            'Integration': 'Entegrasyon',
            'Prepared': 'Hazır',
            'Back': 'Geri',
            'Include in Excel': 'Excel\'e dahil et',
            'International': 'Uluslararası',
            'Conversation': 'Görüşme',
            'Meeting': 'Toplantı',
            'Type': 'Türü',
            'Record Not Found': 'Kayıt bulunamadı',
            'No matching records were found.': 'Aradığınız kriterlere uygun kayıt bulunamadı.',
            'All Status': 'Tüm Durum',
            'All Categories': 'Tüm Kategoriler',
            'All Record Types': 'Tüm Türler',
            'Title': 'Başlık',
            'Notes': 'Notlar',
            'Approved By': 'Onaylayan',
            'Approval Date': 'Onay Tarihi',
            'Created By': 'Oluşturan',
            'Company Name': 'Firma Adı',
            'Open Details': 'Detay Aç',
            'No data found.': 'Veri bulunamadı.',
            'No result found': 'Sonuç bulunamadı',
            'Awaiting': 'Bekleyen',
            'Dismissed': 'Reddedilen',
            'Back Out': 'Vazgeç',
            'Cancel Action': 'İptal Et',
            'Mail': 'Mail',
            'Track the current list and workflow steps on a single screen.': 'Mevcut liste ve süreç adımları tek ekranda izlenir.',
            'Saved': 'Kaydedilen',
            'New Ticket': 'Yeni Ticket',
            'Search by company name or developer name...': '🔍 Firma adı, yazılımcı adı ile ara...',
            'Search status...': 'Durum ara...',
            'Search requester...': 'Talep eden ara...',
            'Not selected': 'Seçilmedi',
            'Ticket Details': 'Ticket Detayı',
            'Status:': 'Durum:',
            'Created By:': 'Oluşturan:',
            'Created On:': 'Oluşturma Tarihi:',
            'Customer Selections:': 'Müşteri Seçimleri:',
            'Payment Types:': 'Ödeme Türleri:',
            'Payment Methods:': 'Ödeme Yöntemleri:',
            'Website Type:': 'Web Sitesi Tipi:',
            'Decision:': 'Karar:',
            'Decision Maker:': 'Karar Veren:',
            'Note:': 'Not:',
            'Date:': 'Tarih:',
            'Description:': 'Açıklama:',
            '1) Operation 1:': '1) Operasyon 1:',
            '2) Compliance:': '2) Uyum:',
            '3) Operation 2:': '3) Operasyon 2:',
            '4) Field:': '4) Saha:',
            'Was the email sent?': 'Mail Gönderildi Mi:',
            'Go-Live Date:': 'Canlı Açıldı Tarihi:',
            'Assign Person / Status (Admin)': 'Kişi / Durum Ata (Admin)',
            'Select operation': 'Operasyon seçiniz',
            'Select status': 'Durum seçiniz',
            'Reason for change': 'Değişiklik nedeni',
            'Close': 'Kapat',
            'Operation 1 - Can Integrate': 'Operasyon 1 - Entegre Olur',
            'Operation 1 - Cannot Integrate': 'Operasyon 1 - Entegre Olmaz',
            'Operation 2 - Email Sent': 'Operasyon 2 - Mail Gönderildi',
            'Operation 2 - Email Not Sent': 'Operasyon 2 - Mail Gönderilmedi',
            'Integration Completed': 'Entegrasyon Tamamlandı',
            'Approve Go-Live': 'Canlı Açılışı Onayla',
            'Missing Documents Added': 'Eksik Evrakı Ekledim',
            'Go-Live Approval': 'Canlı Açılış Onayı',
            'Live Environment ID': 'Canlı Ortam ID',
            'Enter digits only': 'Sadece rakam girin',
            'Only digits can be entered in this field.': 'Bu alana sadece rakam girilebilir.',
            'Optional': 'Opsiyonel',
            'Confirm': 'Onayla',
            'No ticket selected.': 'Ticket seçilmedi.',
            'You must select a person or status.': 'Kişi veya durum seçmelisiniz.',
            'Reason for change is required.': 'Değişiklik nedeni zorunludur.',
            'Operation completed.': 'İşlem tamamlandı.',
            'An error occurred while changing the assignment.': 'Atama değişikliği sırasında bir hata oluştu.',
            'Operation 1 note (optional):': 'Operasyon 1 notu (opsiyonel):',
            'An error occurred during the Operation 1 decision.': 'Operasyon 1 kararı sırasında bir hata oluştu.',
            'Operation 1 rejection note (required):': 'Operasyon 1 ret notu (zorunlu):',
            'Rejection note is required.': 'Ret notu zorunludur.',
            'Operation 2 note (optional):': 'Operasyon 2 notu (opsiyonel):',
            'An error occurred during the Operation 2 decision.': 'Operasyon 2 kararı sırasında bir hata oluştu.',
            'Has the integration been completed? The ticket will be moved to the Field Go-Live Pending status.': 'Entegrasyon tamamlandı mı? Ticket Saha Canli Bekleniyor durumuna alınacaktır.',
            'An error occurred while completing the integration.': 'Entegrasyon tamamlama işlemi sırasında bir hata oluştu.',
            'Live environment ID is required.': 'Canlı ortam ID zorunludur.',
            'Live environment ID must contain digits only.': 'Canlı ortam ID sadece rakamlardan oluşmalıdır.',
            'An error occurred during go-live approval.': 'Canlı açılış onayı sırasında bir hata oluştu.',
            'Cancellation reason is required:': 'İptal nedeni zorunludur:',
            'Cancellation reason is required.': 'İptal nedeni zorunludur.',
            'An error occurred during cancellation.': 'İptal işlemi sırasında bir hata oluştu.',
            'Do you confirm that the missing document has been completed? The ticket will be sent back for compliance approval.': 'Eksik evrakın tamamlandığını onaylıyor musunuz? Ticket yeniden uyum onayına gönderilecektir.',
            'An error occurred while completing the missing document process.': 'Eksik evrak tamamlama işlemi sırasında bir hata oluştu.'
        },
        en: {
            'Operasyon Takip': 'Operations Tracking',
            'Operasyon Takip Sistemi': 'Operations Tracking System',
            'Ana Sayfa': 'Home',
            'Müşteriler': 'Customers',
            'Detaylar': 'Details',
            'Excel Aktarım': 'Excel Import',
            'Toplantı Notları': 'Meeting Notes',
            'İş Takip': 'Work Tracking',
            'Ticketlar': 'Tickets',
            'Hatalar': 'Issues',
            'İzin Takip': 'Leave Tracking',
            'Filtreler': 'Filters',
            'Firma, durum veya talep eden kişiye göre hızlı filtreleme yapın.': 'Quickly filter by company, status, or requester.',
            'Talep Eden': 'Requester',
            'Duruma göre filtrele': 'Filter by status',
            'Talep edene göre filtrele': 'Filter by requester',
            'Henüz ticket bulunmamaktadır.': 'No tickets yet.',
            'Ticket Listesi': 'Ticket List',
            'Detay, durum ve süreç bilgileri tek tabloda gösterilir.': 'Details, status, and workflow information are displayed in a single table.',
            'Atanan Operasyon': 'Assigned Operation',
            'Uyum Kararı Veren': 'Compliance Decision Maker',
            'İrtibat': 'Contact',
            'İşlem': 'Action',
            'Evet': 'Yes',
            'Hayır': 'No',
            'Operasyon 1 Onay Bekleniyor': 'Operation 1 Approval Pending',
            'Operasyon 2 Onay Bekleniyor': 'Operation 2 Approval Pending',
            'Uyum Onay Bekleniyor': 'Compliance Approval Pending',
            'Musteri Kaydedildi': 'Customer Recorded',
            'Saha Canli Bekleniyor': 'Field Go-Live Pending',
            'Eksik Evrak Bekleniyor': 'Missing Documents Pending',
            'Parametreler': 'Parameters',
            'Kullanıcı Listesi': 'User List',
            'Giriş': 'Sign In',
            'Çıkış Yap': 'Sign Out',
            'Operasyon süreçlerini tek merkezden yönetin.': 'Manage operations from a single hub.',
            'Müşteri, ticket, hata, toplantı notları ve iş takip akışlarını daha düzenli ve hızlı şekilde yönetin.': 'Manage customers, tickets, issues, meeting notes, and work tracking flows in a faster and more organized way.',
            'Müşteri Yönetimi': 'Customer Management',
            'Ticket Takibi': 'Ticket Tracking',
            'Güvenli giriş ile rol bazlı ekranlara yönlendirme yapılır.': 'Secure sign-in provides role-based screen routing.',
            'Güvenli Giriş': 'Secure Sign-In',
            'Hoş geldiniz': 'Welcome',
            'Devam etmek için hesabınızla giriş yapın': 'Sign in with your account to continue',
            'Kullanıcı Adı': 'Username',
            'Kullanıcı adınızı girin': 'Enter your username',
            'Şifre': 'Password',
            'Şifrenizi girin': 'Enter your password',
            'Beni Hatırla': 'Remember Me',
            'Giriş Yap': 'Sign In',
            'Kayıt Ol': 'Register',
            'Yeni Hesap Oluştur': 'Create New Account',
            'E-posta': 'Email',
            'Kullanıcı Tipi': 'User Type',
            'Kaydol': 'Register',
            'Zaten hesabın var mı? Giriş yap': 'Already have an account? Sign in',
            'Karşılama Sayfası': 'Welcome Page',
            'BT Operasyon Takip sistemine başarıyla giriş yaptın.': 'You have successfully signed in to the IT Operations Tracking system.',
            'Panele Giriş Yap': 'Enter Dashboard',
            'Yönlendiriliyor...': 'Redirecting...',
            'Sistemi başlatıyor...': 'Starting the system...',
            'Veritabanı bağlantısı kuruluyor...': 'Establishing the database connection...',
            'Modüller yükleniyor...': 'Loading modules...',
            'Dashboard hazırlanıyor...': 'Preparing the dashboard...',
            'Aylık Filtre': 'Monthly Filter',
            'Uygula': 'Apply',
            'Toplam Müşteri': 'Total Customers',
            'Seçili Ay Müşteri': 'Selected Month Customers',
            'Aylık Eklenen': 'Added This Month',
            'Aktif Müşteri': 'Active Customers',
            'Seçili Ay Aktif Müşteri': 'Selected Month Active Customers',
            'Genel Portföy': 'Overall Portfolio',
            'Jira Açık': 'Open Jira Items',
            'Toplam Hata': 'Total Issues',
            'Müşteri Durumu': 'Customer Status',
            'Seçili aydaki durum dağılımı': 'Status distribution for the selected month',
            'Aylık Müşteri Artışı': 'Monthly Customer Growth',
            'Son 6 ay yeni müşteri trendi': 'New customer trend over the last 6 months',
            'Jira Durumu': 'Jira Status',
            'Bekleyen, aktif ve tamamlanan işler': 'Pending, active, and completed work items',
            'Ticket Durumu': 'Ticket Status',
            'Onay bekleyen, onaylanan ve reddedilenler': 'Awaiting approval, approved, and rejected items',
            'Sistem Ticketları': 'System Tickets',
            'Ticket Bulunmuyor': 'No Tickets Found',
            'Henüz hiç ticket oluşturulmamıştır.': 'No tickets have been created yet.',
            'Ticket Arama': 'Ticket Search',
            'Web sitesi, yazılımcı, mail veya oluşturan ile filtrele': 'Filter by website, developer, email, or creator',
            'Arayın...': 'Search...',
            'Dosya adı / müşteri / açıklama / ekleyen...': 'File name / customer / description / added by...',
            'Temizle': 'Clear',
            'Firma Ara': 'Search Company',
            'Firma adı yazın...': 'Enter company name...',
            'Kimden Geldi': 'Requested By',
            'İş akışını daha hızlı yönetin': 'Open operation flow faster',
            'Kişi bazlı görünüm, hızlı filtreleme, görev durumu güncelleme ve detay takibini tek ekranda yönetin.': 'Manage person-based view, quick filtering, task status updates, and detail tracking on a single screen.',
            'Görünüm:': 'View:',
            'Tüm Kişiler': 'All People',
            'Müşteri Ara': 'Search Customer',
            'Firma, yetkili, telefon, teknoloji...': 'Company, contact, phone, technology...',
            'Excel Ay (boş = Tüm Aylar)': 'Excel Month (empty = All Months)',
            'Dosya Seç': 'Choose File',
            'Dosya seçilmedi': 'No file selected',
            'Excel Ay': 'Excel Month',
            'Tüm Excel': 'Full Excel',
            'Yeni Müşteriler Excel': 'New Customers Excel',
            'Filtrele': 'Filter',
            'Müşteri': 'Customer',
            'Yetkili': 'Authorized Person',
            'Durum': 'Status',
            'Durum *': 'Status *',
            'Durum değiştirirken açıklama zorunludur.': 'Description is required when changing status.',
            'Talep Sahibi': 'Requester',
            'Telefon': 'Phone',
            'Açıklama': 'Description',
            'Kayıt Tarihi': 'Created Date',
            'Durum Tarihi': 'Status Date',
            'Kaynak': 'Source',
            'Site URL': 'Website URL',
            'Teknoloji': 'Technology',
            'Firma Adı *': 'Company Name *',
            'Firma Yetkilisi': 'Authorized Contact',
            'Siteyi Aç': 'Open Website',
            'Toplam': 'Total',
            'Gösterilen': 'Displayed',
            'Sayfa': 'Page',
            'Önceki': 'Previous',
            'Sonraki': 'Next',
            'Ara': 'Search',
            'Arama': 'Search Section',
            'Müşteri, ekleyen veya not içeriğine göre filtreleyin.': 'Filter by customer, creator, or note content.',
            'Tümü': 'All',
            'Detayları Aç': 'Open Details',
            'Müşteri Düzenle': 'Edit Customer',
            'Seçiniz': 'Select',
            'İptal': 'Cancel',
            'Güncelle': 'Update',
            'Sil': 'Delete',
            'Tarih Aralığı': 'Date Range',
            'İş Ekle': 'Add Work Item',
            'Temel bilgilerle hızlı kayıt oluşturun': 'Create a new record quickly with key information',
            'Varsayılan durum: Beklemede': 'Default status: Pending',
            'Filtreler ve görünüm': 'Filters and view',
            'Kişi, arama ve kolon görünümünü tek panelden yönetin': 'Manage person, search, and column view from a single panel',
            'Kişi': 'Person',
            'Kolon Filtre': 'Column Filter',
            'Ek durum kolonları': 'Extra status columns',
            'İş Tipi': 'Work Type',
            'Talep Konusu': 'Request Subject',
            'Talep Açan': 'Requested By',
            'Takip Eden': 'Assigned To',
            'Müşteri ara...': 'Search customer...',
            'Yazarak ara...': 'Search by typing...',
            'Konu / Açan / Takip Eden / Durum...': 'Subject / Requester / Assignee / Status...',
            'Bu Sayfa': 'This Page',
            'Aksiyon bekleyen kayıt': 'Records awaiting action',
            'Devam eden iş': 'Ongoing work',
            'Kapanmış iş': 'Closed work',
            'Aksiyon bekleyen kayıtlar': 'Records awaiting action',
            'Çalışılan işler': 'In-progress work',
            'Sonuçlanan işler': 'Completed work',
            'Bu duruma ait kayıtlar': 'Records for this status',
            'Detay': 'Details',
            'Reddedildi': 'Rejected',
            'Beklemede': 'Pending',
            'Aktif': 'Active',
            'Pasif': 'Inactive',
            'Tamamlandı': 'Completed',
            'Döküman İletildi': 'Document Sent',
            'Onay Bekleyen': 'Awaiting Approval',
            'Onaylanan': 'Approved',
            'Açık': 'Open',
            'Kapalı': 'Closed',
            'Askıda': 'On Hold',
            'Diğer': 'Other',
            'Hata': 'Issue',
            'Hata Listesi': 'Issue List',
            'Yeni': 'New',
            'Kayıt yok.': 'No records found.',
            'Henüz kayıt yok.': 'No records yet.',
            'Aradığınız kriterlere uygun müşteri bulunamadı.': 'No customers found for the selected criteria.',
            'Excel (JiraId Gruplu)': 'Excel (Grouped by JiraId)',
            'Excel (Tüm Aylar)': 'Excel (All Months)',
            'Geliştirme / Eğitim Listesi': 'Development / Training List',
            'Excel İndir': 'Download Excel',
            'Tüm Aylar': 'All Months',
            'Ana içeriğe geç': 'Skip to main content',
            'Error': 'Error',
            'Error.': 'Error.',
            'An error occurred while processing your request.': 'An error occurred while processing your request.',
            'Request ID:': 'Request ID:',
            'Development Mode': 'Development Mode',
            'Görüşme Detayları': 'Conversation Details',
            'Müşteri kartlarından seçim yaparak detayları açabilirsiniz.': 'You can open details by selecting from customer cards.',
            'Toplam:': 'Total:',
            'Beklemede:': 'Pending:',
            'Aktif:': 'Active:',
            'Döküman İletildi:': 'Document Sent:',
            'Müşteri #': 'Customer #',
            'Kayıt:': 'Record:',
            'Son Detay': 'Latest Detail',
            'Son Yorumu Ekleyen': 'Added Latest Comment',
            'Site': 'Site',
            'Son Yorum': 'Latest Comment',
            'Son İş / Görüşülen': 'Latest Work / Contact',
            'Seçilen Müşteri:': 'Selected Customer:',
            '+ Yeni Müşteri': '+ New Customer',
            'Kullanıcı Yönetimi': 'User Management',
            'İşlemler': 'Actions',
            '+ Yeni Kullanıcı Ekle': '+ Add New User',
            'Excel\'e İndir': 'Download Excel File',
            'Yeni Hata Ekle': 'Add New Issue',
            'Hata adı, açıklama veya müşteri ara...': 'Search issue name, description, or customer...',
            'Tüm Durumlar': 'All Statuses',
            'Tüm Kategoriler': 'All Categories',
            'Yazılım': 'Software',
            'Donanım': 'Hardware',
            'Ağ': 'Network',
            'Veritabanı': 'Database',
            'Kategori': 'Category',
            'Atanan Kullanıcı': 'Assigned User',
            'Tarih': 'Date',
            'Hatalar Yönetimi': 'Issue Management',
            'Excel yükleyin, veriler veritabanına kaydedilsin ve eşleşen müşterilerin detaylarına işlensin.': 'Upload an Excel file, save its data to the database, and process matching customer details.',
            'Eşleşen': 'Matched',
            'Eşleşmeyen': 'Unmatched',
            'Parametre Yönetimi': 'Parameter Management',
            'Sistem içindeki tür ve parametre değerlerini merkezi olarak yönetin': 'Manage system types and parameter values centrally',
            'Hoşgeldiniz': 'Welcome',
            'İstek ID:': 'Request ID:',
            'İsteğiniz işlenirken bir hata oluştu.': 'An error occurred while processing your request.',
            'Geliştirme Modu': 'Development Mode',
            'Ana içeriğe geç': 'Skip to main content',
            'ASP.NET Core ile web uygulamaları geliştirme hakkında bilgi edinin.': 'Learn about building Web apps with ASP.NET Core.',
            'Toplantı Başlığı': 'Meeting Title',
            'Toplantı Tutanağı': 'Meeting Minutes',
            'Aksiyon Sahibi': 'Action Owner',
            'Hedef Tarihi': 'Target Date',
            'Katılımcı': 'Participants',
            'Hata Adı': 'Issue Name',
            'Müşteri Web Sitesi': 'Customer Website',
            'Yazılımcı': 'Developer',
            'Hata Çözüm': 'Issue Resolution',
            'Geliştirme': 'Development',
            'Eğitim': 'Training',
            'Entegrasyon': 'Integration',
            'Hazır': 'Prepared',
            'Geri': 'Back',
            'Kayıt bulunamadı': 'Record Not Found',
            'Aradığınız kriterlere uygun kayıt bulunamadı.': 'No matching records were found.',
            'Tüm Durum': 'All Status',
            'Tüm Türler': 'All Record Types',
            'Başlık': 'Title',
            'Notlar': 'Notes',
            'Onaylayan': 'Approved By',
            'Onay Tarihi': 'Approval Date',
            'Oluşturan': 'Created By',
            'Firma Adı': 'Company Name',
            'Detay Aç': 'Open Details',
            'Veri bulunamadı.': 'No data found.',
            'Sonuç bulunamadı': 'No result found',
            'Toplam Kayıt': 'Total Records',
            'Toplam Tür': 'Total Types',
            'Yeni Parametre': 'New Parameter',
            'Yeni Tür': 'New Type',
            'Tür Ekle': 'Add Type',
            'Yeni Tür Ekle': 'Add New Type',
            'Durum Yönetimi': 'Status Management',
            'Parametre Listesi': 'Parameter List',
            'Parametre Adı': 'Parameter Name',
            'Parametre Türü': 'Parameter Type',
            'Tür': 'Type',
            'Sıra': 'Order',
            'Yukarı': 'Move Up',
            'Aşağı': 'Move Down',
            'İş Takip kolonu olarak görünür': 'Visible in Work Tracking columns',
            'İş Takip ekranındaki kolon sırasını ve durum görünümünü buradan yönetin': 'Configure the order of columns and status visibility for the Work Tracking screen here.',
            'Mevcut bir türe yeni parametre değeri ekleyin': 'Add a new parameter value to an existing type.',
            'Parametre grupları için yeni tür tanımlayın': 'Define a new type for parameter groups.',
            'Toplantı Türü': 'Meeting Type',
            'İş Takip kolonu': 'Work Tracking column',
            'İş Takip\'e Dön': 'Back to Work Tracking',
            'Excel\'e dahil edilmeyen geliştirme ve eğitim işleri burada listelenir.': 'Excluded development and training work items are listed here.',
            'Kayıt bulunamadı.': 'No record found.',
            'Kayıt bulunamadı': 'No record found',
            'Yeni Parametre Değeri': 'New Parameter Value',
            'Kayıt No:': 'Record No:',
            'Görev eklendi.': 'Task added.',
            'Durum güncellendi.': 'Status updated.',
            'Takip eden güncellendi.': 'Assigned user updated.',
            'Yorum eklendi.': 'Comment added.',
            'Kayıt silindi.': 'Record deleted.',
            'Yorum boş olamaz.': 'Comment cannot be empty.',
            'Geçersiz durum güncellemesi.': 'Invalid status update.',
            'Geçersiz iş tipi seçildi.': 'Invalid work type selected.',
            'Geçersiz kayıt.': 'Invalid record.',
            'Görev bulunamadı.': 'Task not found.',
            'Jira ID ve Talep Konusu zorunludur.': 'Jira ID and Request Subject are required.',
            'Seçilen müşteri bulunamadı.': 'Selected customer could not be found.',
            'Talep Konusu zorunludur.': 'Request Subject is required.',
            'Bu müşteri için döküman durumu güncellenemez.': 'Document status cannot be updated for this customer.',
            'Firma adı zorunludur.': 'Company name is required.',
            'Geçerli bir durum seçiniz.': 'Please select a valid status.',
            'Müşteri bulunamadı.': 'Customer not found.',
            'Döküman durumu güncellendi.': 'Document status updated.',
            'Müşteri başarıyla eklendi!': 'Customer added successfully!',
            'Müşteri başarıyla güncellendi!': 'Customer updated successfully!',
            'Excel içinde aktarılacak kayıt bulunamadı.': 'No records were found to import from Excel.',
            'Lütfen bir Excel dosyası seçiniz.': 'Please select an Excel file.',
            'Sadece .xlsx dosyaları yüklenebilir.': 'Only .xlsx files can be uploaded.',
            'Not içeriği güncellendi': 'Note content updated',
            'Hata Adı': 'Issue Name',
            'Oluşturan': 'Created By',
            'Müşteri Web Sitesi': 'Company Website',
            'Yazılımcı': 'Developer Name',
            'Görüşme Tarihi': 'Conversation Date',
            'Tarih Saat': 'Date Time',
            'İş / Görüşülen': 'Business / Contact',
            'Ekleyen': 'Added By',
            'Excel Aktarım': 'Excel Transfer',
            'Excel Yükle': 'Upload Excel',
            'Şu sütunlardan biri varsa aktarım yapılır: Müşteri/Firma, Tarih, İş-Görüşülen, Açıklama, Ekleyen.': 'Import starts if one of these columns exists: Customer/Company, Date, Work-Contact, Description, Added By.',
            'Eşleşen Müşteriler': 'Matched Customers',
            'Eşleşmeyen Müşteriler': 'Unmatched Customers',
            'Başarıyla oluşturuldu.': 'Created successfully.',
            'Başarıyla güncellendi.': 'Updated successfully.',
            'Başarıyla silindi.': 'Deleted successfully.',
            'Mevcut liste ve süreç adımları tek ekranda izlenir.': 'Track the current list and workflow steps on a single screen.',
            'Bekleyen': 'Awaiting',
            'Kaydedilen': 'Saved',
            'Reddedilen': 'Dismissed',
            'Vazgeç': 'Back Out',
            'İptal Et': 'Cancel Action',
            'Yeni Ticket': 'New Ticket',
            '🔍 Firma adı, yazılımcı adı ile ara...': 'Search by company name or developer name...',
            'Durum ara...': 'Search status...',
            'Talep eden ara...': 'Search requester...',
            'Seçilmedi': 'Not selected',
            'Mail': 'Mail',
            'Ticket Detayı': 'Ticket Details',
            'Durum:': 'Status:',
            'Oluşturan:': 'Created By:',
            'Oluşturma Tarihi:': 'Created On:',
            'Müşteri Seçimleri:': 'Customer Selections:',
            'Ödeme Türleri:': 'Payment Types:',
            'Ödeme Yöntemleri:': 'Payment Methods:',
            'Web Sitesi Tipi:': 'Website Type:',
            'Karar:': 'Decision:',
            'Karar Veren:': 'Decision Maker:',
            'Not:': 'Note:',
            'Tarih:': 'Date:',
            'Açıklama:': 'Description:',
            '1) Operasyon 1:': '1) Operation 1:',
            '2) Uyum:': '2) Compliance:',
            '3) Operasyon 2:': '3) Operation 2:',
            '4) Saha:': '4) Field:',
            'Mail Gönderildi Mi:': 'Was the email sent?',
            'Canlı Açıldı Tarihi:': 'Go-Live Date:',
            'Kişi / Durum Ata (Admin)': 'Assign Person / Status (Admin)',
            'Operasyon seçiniz': 'Select operation',
            'Durum seçiniz': 'Select status',
            'Değişiklik nedeni': 'Reason for change',
            'Kapat': 'Close',
            'Operasyon 1 - Entegre Olur': 'Operation 1 - Can Integrate',
            'Operasyon 1 - Entegre Olmaz': 'Operation 1 - Cannot Integrate',
            'Operasyon 2 - Mail Gönderildi': 'Operation 2 - Email Sent',
            'Operasyon 2 - Mail Gönderilmedi': 'Operation 2 - Email Not Sent',
            'Entegrasyon Tamamlandı': 'Integration Completed',
            'Canlı Açılışı Onayla': 'Approve Go-Live',
            'Eksik Evrakı Ekledim': 'Missing Documents Added',
            'Canlı Açılış Onayı': 'Go-Live Approval',
            'Canlı Ortam ID': 'Live Environment ID',
            'Sadece rakam girin': 'Enter digits only',
            'Bu alana sadece rakam girilebilir.': 'Only digits can be entered in this field.',
            'Opsiyonel': 'Optional',
            'Onayla': 'Confirm',
            'Ticket seçilmedi.': 'No ticket selected.',
            'Kişi veya durum seçmelisiniz.': 'You must select a person or status.',
            'Değişiklik nedeni zorunludur.': 'Reason for change is required.',
            'İşlem tamamlandı.': 'Operation completed.',
            'Atama değişikliği sırasında bir hata oluştu.': 'An error occurred while changing the assignment.',
            'Operasyon 1 notu (opsiyonel):': 'Operation 1 note (optional):',
            'Operasyon 1 kararı sırasında bir hata oluştu.': 'An error occurred during the Operation 1 decision.',
            'Operasyon 1 ret notu (zorunlu):': 'Operation 1 rejection note (required):',
            'Ret notu zorunludur.': 'Rejection note is required.',
            'Operasyon 2 notu (opsiyonel):': 'Operation 2 note (optional):',
            'Operasyon 2 kararı sırasında bir hata oluştu.': 'An error occurred during the Operation 2 decision.',
            'Entegrasyon tamamlandı mı? Ticket Saha Canli Bekleniyor durumuna alınacaktır.': 'Has the integration been completed? The ticket will be moved to the Field Go-Live Pending status.',
            'Entegrasyon tamamlama işlemi sırasında bir hata oluştu.': 'An error occurred while completing the integration.',
            'Canlı ortam ID zorunludur.': 'Live environment ID is required.',
            'Canlı ortam ID sadece rakamlardan oluşmalıdır.': 'Live environment ID must contain digits only.',
            'Canlı açılış onayı sırasında bir hata oluştu.': 'An error occurred during go-live approval.',
            'İptal nedeni zorunludur:': 'Cancellation reason is required:',
            'İptal nedeni zorunludur.': 'Cancellation reason is required.',
            'İptal işlemi sırasında bir hata oluştu.': 'An error occurred during cancellation.',
            'Eksik evrakın tamamlandığını onaylıyor musunuz? Ticket yeniden uyum onayına gönderilecektir.': 'Do you confirm that the missing document has been completed? The ticket will be sent back for compliance approval.',
            'Eksik evrak tamamlama işlemi sırasında bir hata oluştu.': 'An error occurred while completing the missing document process.',
            'Müşteri Toplantı Notları': 'Customer Meeting Notes',
            'Toplantı kayıtlarını oluşturun, inceleyin ve dışa aktarın.': 'Create, review, and export meeting records.',
            'Tür Seçenekleri': 'Type Options',
            'Müşteri, ekleyen veya not içeriğine göre filtreleyin.': 'Filter by customer, creator, or note content.',
            'Ara (Müşteri / Ekleyen / Not)': 'Search (Customer / Creator / Note)',
            'Yeni Toplantı Kaydı': 'New Meeting Record',
            'Toplantı bilgilerini ve konu/hedef tarihlerini tek formdan kaydedin.': 'Save meeting information and subject/target dates from a single form.',
            'Toplantı Başlığı *': 'Meeting Title *',
            'Konum *': 'Location *',
            'Tarih *': 'Date *',
            'Saat *': 'Time *',
            'Hazırlayan *': 'Prepared By *',
            'Katılımcılar *': 'Participants *',
            'Konular ve Hedef Tarihleri *': 'Topics and Target Dates *',
            'Yeni Konu Ekle': 'Add New Topic',
            'Toplantı Notu': 'Meeting Note',
            'Toplantı Tutanağı': 'Meeting Minutes',
            'İsim': 'Name',
            'Toplantı Başlığı:': 'Meeting Title:',
            'Konum:': 'Location:',
            'Saat:': 'Time:',
            'Hazırlayan:': 'Prepared By:',
            'Katılımcılar:': 'Participants:',
            'Excel Dosyası (.xlsx)': 'Excel File (.xlsx)',
            'Eşleşen müşteri bulunursa ilgili kayıt Detaylar sayfasına da eklenir.': 'If a matching customer is found, the related record is also added to the Details page.',
            'Yükle ve Kaydet': 'Upload and Save',
            'İçe Aktarılan Kayıtlar': 'Imported Records',
            'Bu sayfada yüklediğiniz Excel verilerini ayrı olarak görüntüleyebilirsiniz.': 'You can view the Excel data uploaded on this page separately.',
            'Yükleme': 'Upload',
            'Dosya': 'File',
            'Satır': 'Row',
            'Yükleyen': 'Uploader',
            'Eşleşti': 'Matched',
            'Eşleşmedi': 'Not Matched',
            'Detaya Git': 'Go to Details',
            'Henüz aktarılmış kayıt yok.': 'No imported records yet.',
            'Kullanıcı Adı': 'Username',
            'E-posta': 'Email Address',
            'Hata': 'Issue',
            'Atanan': 'Assigned',
            'Detay': 'Detail',
            'Excel\'e İndir': 'Download to Excel',
            'Excel\'e Aktar': 'Export to Excel',
            'Tanımlı izin hakkı': 'Leave allowance defined',
            'Onaylanmış kullanılan izin': 'Approved leave used',
            'Kalan izin': 'Remaining leave',
            'gün': 'day',
            'İzin Talebini Güncelle': 'Update Leave Request',
            'Yeni İzin Talebi': 'New Leave Request',
            'Kaydedilen her değişiklik admin onayına yeniden düşer.': 'Every saved change is sent back to admin approval.',
            'Başlangıç Tarihi': 'Start Date',
            'Bitiş Tarihi': 'End Date',
            'Açıklama / Mazeret': 'Description / Reason',
            'Güncelle ve Onaya Gönder': 'Update and Send for Approval',
            'Talep Oluştur': 'Create Request',
            'Kullanıcı İzin Hakları': 'User Leave Balances',
            'Sadece operasyon kullanıcıları listelenir.': 'Only operation users are listed.',
            'Kullanıcı': 'User',
            'Toplam Hak': 'Total Allowance',
            'Kullanılan': 'Used',
            'Kalan': 'Remaining',
            'Yeni Hak Tanımı': 'New Allowance Definition',
            'Operasyon kullanıcısı bulunamadı.': 'No operation user found.',
            'Kaydet': 'Save',
            'Tüm İzin Talepleri': 'All Leave Requests',
            'İzin Taleplerim': 'My Leave Requests',
            'Düzenlenen kayıtlar tekrar beklemeye alınır.': 'Edited records are moved back to pending.',
            'Talep Sahibi': 'Request Owner',
            'Gün': 'Day',
            'Admin Notu': 'Admin Note',
            'Talep Tarihi': 'Request Date',
            'Henüz izin talebi bulunmuyor.': 'No leave request exists yet.',
            'Düzenle': 'Edit',
            'Onay notu veya red mazereti...': 'Approval note or rejection reason...',
            'Reddet': 'Reject',
            'Operasyon izin taleplerini yönetin ve izin hakkı tanımlayın.': 'Manage operation leave requests and define leave balances.',
            'İzin talebinizi oluşturun, güncelleyin ve admin kararını takip edin.': 'Create or update your leave request and track the admin decision.',
            'Toplam Parametre': 'Total Parameter',
            'Tanımlı türler ve parametre değerleri': 'Defined types and parameter values',
            'Ad': 'Name',
            'Yukarı / aşağı butonları kolon sırasını belirler.': 'The up / down buttons define the column order.',
            'Henüz': 'Yet',
            'türünde parametre tanımlı değil.': 'No parameter is defined yet for the Status type.',
            'değerleri parametrelerden çekilir.': 'The values of',
            'Yeni Ticket Oluştur': 'New Ticket Create',
            'Müşteri, ödeme ve teknik bilgileri tek form üzerinden eksiksiz girin.': 'Enter customer, payment, and technical details completely through a single form.',
            'Süreç başlangıcı: Operasyon 1': 'Process starts: Operation 1',
            'Müşteri tipi zorunlu': 'Customer type is required',
            'Ticket Bilgileri': 'Ticket Information',
            'Zorunlu alanları doldurup kaydı oluşturun.': 'Fill in the required fields and create the record.',
            'Genel Bilgiler': 'General Information',
            'Firma Adı': 'Company Name',
            'Müşteri Ne Kullanacak?': 'What Will the Customer Use?',
            'Web Sitesi Kim Tarafından Yazıldı?': 'Who Developed the Website?',
            'Hangi Ödeme Yöntemleri Kullanılacak?': 'What Payment Methods Will Be Used?',
            'Yazılımcı Adı': 'Developer First Name',
            'Yazılımcı Soyadı': 'Developer Last Name',
            'Mail Adresi': 'Email Address',
            'İrtibat Numarası': 'Contact Number',
            'Süreç Özeti': 'Process Summary',
            'Kayıt sonrası ticket doğrudan Operasyon 1 onay adımına düşer. Gerekli alanları eksiksiz doldurmanız süreci hızlandırır.': 'After submission, the ticket directly moves to the Operation 1 approval step. Completing all required fields accelerates the process.',
            'Dikkat Edilecekler': 'Things to Watch',
            'Aynı firma, site veya mail için açık ticket varsa kayıt engellenir.': 'If there is already an open ticket for the same company, website, or email, the record is blocked.',
            'Firma, web sitesi ve iletişim bilgileri eksiksiz girilmelidir.': 'Company, website, and contact details must be entered completely.',
            'Ödeme ve web sitesi seçimleri doğru işaretlenmelidir.': 'Payment and website selections must be marked correctly.',
            'Aksiyonlar': 'Actions',
            'Aşağıdaki Gönder Butonuna Git': 'Go to the Submit Button Below',
            'Listeye Dön': 'Back to List',
            'Ticket Gönder': 'Send Ticket',
            'En az bir konu girmelisiniz.': 'At least one topic must be entered.',
            'Bu kolonda gösterilecek kayıt yok.': 'No records to display in this column.',
            'Bu kaydı silmek istiyor musunuz?': 'Do you want to delete this record?',
            'Toplantı kaydı oluşturuldu.': 'Meeting record created.',
            'Kayıt doğrulanamadı.': 'Record validation failed.',
            'Durum boş olamaz.': 'Empty status',
            'Görev bulunamadı.': 'Task not found',
            'Durum güncellendi.': 'Status updated',
            'Hata:': 'Error:',
            'Geçersiz kayıt.': 'Invalid model',
            'Kayıt bulunamadı.': 'Record not found',
            'Toplantı Tutanağı': 'Meeting Minutes',
            'Toplantı Başlığı': 'Meeting Title',
            'Konum': 'Location',
            'Konu': 'Topic',
            'Hedef Tarihi': 'Target Date',
            'Hazırlayan': 'Prepared By',
            'Aksiyon Sahibi': 'Action Owner',
            'Excel okunamadı:': 'Excel could not be read:',
            'Ticket oluşturuldu. Operasyon 1 onayı bekleniyor.': 'Ticket created. Operation 1 approval is pending.',
            'Geçersiz ticket ID.': 'Invalid ticket ID.',
            'Ticket bulunamadı.': 'Ticket not found.',
            'Bu ticket size atanmadığı için işlem yapamazsınız.': 'You cannot perform this action because this ticket is not assigned to you.',
            'Operasyon 1 kararı kaydedildi. Uyum onayı bekleniyor.': 'Operation 1 decision saved. Compliance approval is pending.',
            'Operasyon 2 kararı kaydedildi.': 'Operation 2 decision saved.',
            'Entegrasyon tamamlandı. Ticket tekrar Saha Canli Bekleniyor durumuna alındı.': 'Integration completed. Ticket was moved back to Field Go-Live Pending status.',
            'Canlı açılış kaydedildi. Müşteri kaydı oluşturuldu veya güncellendi.': 'Go-live recorded. Customer record was created or updated.',
            'Ticket reddedildi.': 'Ticket rejected.',
            'Eksik evrak tamamlandı. Ticket yeniden uyum onayına gönderildi.': 'Missing document completed. Ticket was sent back to compliance approval.',
            'Geçersiz istek.': 'Request is invalid.',
            'Değişiklik nedeni zorunlu.': 'Change reason is required.',
            'Değişiklik nedeni maksimum 500 karakter olmalıdır.': 'Change reason must be at most 500 characters.',
            'Seçilen kullanıcı operasyon rolünde değil veya bulunamadı.': 'The selected user is not in the operation role or could not be found.',
            'Seçilen durum geçerli değil.': 'The selected status is not valid.',
            'Kişi veya durum değişikliği yapmadınız.': 'No assignee or status change was made.',
            'Kişi ataması güncellendi.': 'Assignee assignment updated.',
            'Kişi ve durum güncellendi.': 'Assignee and status updated.'
        }
    };

    const patternTranslations = {
        tr: [
            {
                regex: /^Welcome,\s*(.+)!$/,
                replace: (_, name) => `Hoş geldin, ${name}!`
            },
            {
                regex: /^System Tickets \((\d+)\)$/,
                replace: (_, count) => `Sistem Ticketları (${count})`
            },
            {
                regex: /^Person:\s*(.+)$/,
                replace: (_, value) => `Kişi: ${value === 'All' ? 'Tümü' : value}`
            },
            {
                regex: /^Total:\s*(\d+)$/,
                replace: (_, count) => `Toplam: ${count}`
            },
            {
                regex: /^(\d+) records$/,
                replace: (_, count) => `${count} kayıt`
            },
            {
                regex: /^(\d+)\-(\d+) \/ (\d+) records$/,
                replace: (_, from, to, total) => `${from}-${to} / ${total} kayıt`
            },
            {
                regex: /^Page:\s*(\d+)\/(\d+)$/,
                replace: (_, current, total) => `Sayfa: ${current}/${total}`
            },
            {
                regex: /^Total Records:\s*(\d+)$/,
                replace: (_, count) => `Toplam Kayıt: ${count}`
            },
            {
                regex: /^Matched:\s*(\d+)$/,
                replace: (_, count) => `Eşleşen: ${count}`
            },
            {
                regex: /^Unmatched:\s*(\d+)$/,
                replace: (_, count) => `Eşleşmeyen: ${count}`
            },
            {
                regex: /^Record No:\s*#(\d+)$/,
                replace: (_, id) => `Kayıt No: #${id}`
            },
            {
                regex: /^Matched Customers:\s*(\d+)$/,
                replace: (_, count) => `Eşleşen Müşteriler: ${count}`
            },
            {
                regex: /^Unmatched Customers:\s*(\d+)$/,
                replace: (_, count) => `Eşleşmeyen Müşteriler: ${count}`
            },
            {
                regex: /^Matched:\s*(\d+)$/,
                replace: (_, count) => `Eşleşen: ${count}`
            },
            {
                regex: /^Unmatched:\s*(\d+)$/,
                replace: (_, count) => `Eşleşmeyen: ${count}`
            }
        ],
        en: [
            {
                regex: /^Hoş geldin,\s*(.+)!$/,
                replace: (_, name) => `Welcome, ${name}!`
            },
            {
                regex: /^Sistem Ticketları \((\d+)\)$/,
                replace: (_, count) => `System Tickets (${count})`
            },
            {
                regex: /^Kişi:\s*(.+)$/,
                replace: (_, value) => `Person: ${value === 'Tümü' ? 'All' : value}`
            },
            {
                regex: /^Toplam:\s*(\d+)$/,
                replace: (_, count) => `Total: ${count}`
            },
            {
                regex: /^(\d+) kayıt$/,
                replace: (_, count) => `${count} records`
            },
            {
                regex: /^(\d+)\-(\d+) \/ (\d+) kayıt$/,
                replace: (_, from, to, total) => `${from}-${to} / ${total} records`
            },
            {
                regex: /^Sayfa:\s*(\d+)\/(\d+)$/,
                replace: (_, current, total) => `Page: ${current}/${total}`
            },
            {
                regex: /^Toplam Kayıt:\s*(\d+)$/,
                replace: (_, count) => `Total Records: ${count}`
            },
            {
                regex: /^Eşleşen:\s*(\d+)$/,
                replace: (_, count) => `Matched: ${count}`
            },
            {
                regex: /^Eşleşmeyen:\s*(\d+)$/,
                replace: (_, count) => `Unmatched: ${count}`
            },
            {
                regex: /^Kayıt No:\s*#(\d+)$/,
                replace: (_, id) => `Record No: #${id}`
            },
            {
                regex: /^Eşleşen Müşteriler:\s*(\d+)$/,
                replace: (_, count) => `Matched Customers: ${count}`
            },
            {
                regex: /^Eşleşmeyen Müşteriler:\s*(\d+)$/,
                replace: (_, count) => `Unmatched Customers: ${count}`
            },
            {
                regex: /^Eşleşen:\s*(\d+)$/,
                replace: (_, count) => `Matched: ${count}`
            },
            {
                regex: /^Eşleşmeyen:\s*(\d+)$/,
                replace: (_, count) => `Unmatched: ${count}`
            },
            {
                regex: /^Müşteri ID\s+(\d+)\s+bulunamadı$/,
                replace: (_, id) => `Customer ID ${id} was not found`
            },
            {
                regex: /^Customer ID\s+(\d+)\s+was not found$/,
                replace: (_, id) => `Müşteri ID ${id} bulunamadı`
            }
        ]
    };

    function normalizeLanguage(language) {
        return supportedLanguages.has(language) ? language : defaultLanguage;
    }

    function getLanguage() {
        return normalizeLanguage(localStorage.getItem(storageKey) || defaultLanguage);
    }

    function setLanguage(language) {
        localStorage.setItem(storageKey, normalizeLanguage(language));
    }

    function translateValue(value, language) {
        if (!value) {
            return value;
        }

        const exact = exactTranslations[language] || {};
        const patterns = patternTranslations[language] || [];
        const collapsed = value.replace(/\s+/g, ' ').trim();

        if (!collapsed) {
            return value;
        }

        if (Object.prototype.hasOwnProperty.call(exact, collapsed)) {
            const leading = value.match(/^\s*/)?.[0] ?? '';
            const trailing = value.match(/\s*$/)?.[0] ?? '';
            return `${leading}${exact[collapsed]}${trailing}`;
        }

        for (const rule of patterns) {
            if (rule.regex.test(collapsed)) {
                const leading = value.match(/^\s*/)?.[0] ?? '';
                const trailing = value.match(/\s*$/)?.[0] ?? '';
                return `${leading}${collapsed.replace(rule.regex, rule.replace)}${trailing}`;
            }
        }

        return value;
    }

    function processTextNode(node, language) {
        if (!node || !node.parentElement || skipTags.has(node.parentElement.tagName)) {
            return;
        }

        const currentValue = node.nodeValue;
        if (!currentValue || !currentValue.trim()) {
            return;
        }

        if (node.parentElement.hasAttribute('data-no-translate')) {
            return;
        }

        const original = node.__i18nOriginalText || currentValue;
        node.__i18nOriginalText = original;
        const translatedValue = translateValue(original, language);
        if (node.nodeValue !== translatedValue) {
            node.nodeValue = translatedValue;
        }
    }

    function processAttribute(element, attributeName, storageName, language) {
        if (!element.hasAttribute(attributeName)) {
            return;
        }

        const original = element.dataset[storageName] || element.getAttribute(attributeName);
        element.dataset[storageName] = original;
        const translatedValue = translateValue(original, language);
        if (element.getAttribute(attributeName) !== translatedValue) {
            element.setAttribute(attributeName, translatedValue);
        }
    }

    function processElement(element, language) {
        if (!element || element.hasAttribute('data-no-translate')) {
            return;
        }

        processAttribute(element, 'placeholder', 'i18nPlaceholderOriginal', language);
        processAttribute(element, 'title', 'i18nTitleOriginal', language);
        processAttribute(element, 'aria-label', 'i18nAriaLabelOriginal', language);

        if (element instanceof HTMLInputElement) {
            const type = (element.type || '').toLowerCase();
            if (['button', 'submit', 'reset'].includes(type)) {
                const originalValue = element.dataset.i18nValueOriginal || element.value;
                element.dataset.i18nValueOriginal = originalValue;
                const translatedValue = translateValue(originalValue, language);
                if (element.value !== translatedValue) {
                    element.value = translatedValue;
                }
            }
        }
    }

    function translateRoot(root, language) {
        if (!root) {
            return;
        }

        if (root.nodeType === Node.TEXT_NODE) {
            processTextNode(root, language);
            return;
        }

        if (root.nodeType !== Node.ELEMENT_NODE && root.nodeType !== Node.DOCUMENT_NODE) {
            return;
        }

        if (root.nodeType === Node.ELEMENT_NODE) {
            processElement(root, language);
        }

        const walker = document.createTreeWalker(root, NodeFilter.SHOW_ELEMENT | NodeFilter.SHOW_TEXT, null);
        let current = walker.currentNode;
        while (current) {
            if (current.nodeType === Node.TEXT_NODE) {
                processTextNode(current, language);
            } else if (current.nodeType === Node.ELEMENT_NODE) {
                processElement(current, language);
            }
            current = walker.nextNode();
        }
    }

    function translateDocumentTitle(language) {
        if (!document.title) {
            return;
        }

        const originalTitle = document.documentElement.dataset.i18nTitleOriginal || document.title;
        document.documentElement.dataset.i18nTitleOriginal = originalTitle;
        document.title = translateValue(originalTitle, language);
    }

    function updateSwitcherState(language) {
        document.querySelectorAll('[data-language-switch][data-language-value]').forEach(button => {
            const selected = button.getAttribute('data-language-value') === language;
            button.classList.toggle('active', selected);
            button.setAttribute('aria-pressed', selected ? 'true' : 'false');
        });
    }

    function applyLanguage(language) {
        const normalized = normalizeLanguage(language);
        setLanguage(normalized);
        document.documentElement.lang = normalized === 'en' ? 'en' : 'tr';
        document.body?.setAttribute('data-ui-language', normalized);
        window.__uiLocalizationApplying = true;

        try {
            translateDocumentTitle(normalized);
            translateRoot(document.body, normalized);
            updateSwitcherState(normalized);
            document.dispatchEvent(new CustomEvent('ui-language-changed', { detail: { language: normalized } }));
        } finally {
            window.__uiLocalizationApplying = false;
        }
    }

    function initializeSwitcher() {
        if (window.__uiLocalizationSwitcherInitialized) {
            return;
        }

        window.__uiLocalizationSwitcherInitialized = true;

        document.addEventListener('click', function (event) {
            const button = event.target instanceof Element
                ? event.target.closest('[data-language-switch][data-language-value]')
                : null;

            if (!button) {
                return;
            }

            const language = button.getAttribute('data-language-value');
            applyLanguage(language);
        });
    }

    function initializeObserver() {
        if (!document.body || window.__uiLocalizationObserverInitialized) {
            return;
        }

        window.__uiLocalizationObserverInitialized = true;
        const observer = new MutationObserver(mutations => {
            if (window.__uiLocalizationApplying) {
                return;
            }

            const language = getLanguage();
            mutations.forEach(mutation => {
                mutation.addedNodes.forEach(node => translateRoot(node, language));
                if (mutation.type === 'characterData') {
                    processTextNode(mutation.target, language);
                }
            });
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true,
            characterData: true
        });
    }

    function patchBrowserDialogs() {
        if (window.__uiLocalizationDialogsPatched) {
            return;
        }

        window.__uiLocalizationDialogsPatched = true;

        const originalAlert = window.alert.bind(window);
        const originalConfirm = window.confirm.bind(window);

        window.alert = function (message) {
            originalAlert(typeof message === 'string' ? translateValue(message, getLanguage()) : message);
        };

        window.confirm = function (message) {
            return originalConfirm(typeof message === 'string' ? translateValue(message, getLanguage()) : message);
        };
    }

    function bootLocalization() {
        patchBrowserDialogs();
        initializeSwitcher();
        applyLanguage(getLanguage());
        initializeObserver();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', bootLocalization);
    } else {
        bootLocalization();
    }

    window.UiLocalization = {
        applyLanguage,
        getLanguage,
        translate: value => translateValue(value, getLanguage())
    };
})();
