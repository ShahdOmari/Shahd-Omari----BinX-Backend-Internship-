# Technical Q&A — Expected Questions & Answers

---

## Q1: ليش استخدمتِ JWT وما استخدمتِ Session-based auth؟

**الجواب:**
"Session-based auth بتحتاج الـ server يخزن الـ session state —
مشكلة لو عندك أكثر من server instance أو containerized deployment.
JWT self-contained: كل المعلومات بالتوكن نفسه.

الأهم في مشروعي: ضفت `staffProfileId` كـ claim — معناه كل ownership check
بيقرأ من التوكن مباشرة بدون DB query.

العيب الحقيقي لـ JWT: token revocation صعب. لو دكتور انخفض لـ Nurse،
بيضل عنده صلاحيات Doctor لـ 60 دقيقة (مدة التوكن). موثّق كـ edge case
بالـ Sprint 2 backlog."

---

## Q2: شو الفرق بين 401 و403 ومتى تستخدم كل واحد؟

**الجواب:**
"401 Unauthorized: ما بعرف مين أنت — التوكن غلط، منتهي، أو ما في توكن.
403 Forbidden: بعرف مين أنت، والجواب لا.

لو رجعنا 401 بدل 403، بنوحي للمهاجم إنه ممكن يوصل لو عنده credentials صح.
في مشروعي: Nurse مصادق عليها بتاخذ 403 على Doctor-only endpoints —
مش 401 — لأنها authenticated بس مش authorized."

---

## Q3: ليش Cache-Aside وما استخدمتِ Write-Through؟

**الجواب:**
"Write-Through يعني كل write بيحدّث الـ cache والـ DB مع بعض —
أعقد، وبتحتاج تضمن atomic update بين الاثنين.

Cache-Aside أبسط: على الـ write، بس احذف الـ cache. الـ next read يجدده.
للـ patients catalog اللي بيتغير نادراً مقارنة بكثرة قراءته، هاد كافي.

الحماية الإضافية: Absolute Expiration 10 دقائق — حتى لو الـ invalidation
فشل لأي سبب، بعد 10 دقائق الـ cache بيتجدد تلقائياً من الـ DB."

---

## Q4: ليش Composite Index وما عملتِ index على كل column لحاله؟

**الجواب:**
"الـ query على GetCriticalPatients بتعمل:
WHERE RiskLevel = 'Critical' AND RecordedAtUtc = MAX(...)

لو عندي index على RiskLevel لحاله: SQL Server بيستخدمه للفلتر،
بعدين بيرتّب النتائج بالـ RAM.

الـ composite index على (RiskLevel, RecordedAtUtc DESC) يخلي
SQL Server يعمل الاثنين بخطوة وحدة — index seek، مش sort.

كل index بيبطئ الـ writes (INSERT/UPDATE) لأن SQL Server بيحدّثه.
لهيك ما بنحط index على كل column — بس على اللي فيه query patterns حقيقية."

---

## Q5: كيف تأكدتِ إن الـ tests بتغطي الـ security requirements؟

**الجواب:**
"عملت coverage audit يوم 1 من Sprint 4 — راجعت كل endpoint ضد ثلاثة أشياء:
هل في happy path test، error path test، وRBAC test.

اكتشفت إن ownership check ما عنده اختبار إطلاقاً — كان مختبر يدوياً
بالـ Postman بس. ضفت:
- Nurse A تسجّل قراءة → Nurse B تحاول تقرأها → 403
- Nurse A تقرأ قراءتها → 200

هاد مهم لأن RBAC وحده ما بكفي — ممرضتين بنفس الدور
المفروض تشوف بيانات مختلفة."

---

## Q6: شو بتعمل لو Redis وقع بالـ Production؟

**الجواب:**
"الـ CacheService بيعمل try/catch على كل method.
لو GetAsync رجعت exception، بترجع null.
الـ controller بيشوف null → بروح للـ DB.

الـ API تشتغل بشكل طبيعي بدون cache — أبطأ، بس ما بتسقط.
هاد اسمه graceful degradation: cache failure = cache miss, مش 500 error.

موثّق بالـ CacheService comments: 'A cache that takes the API down
with it is worse than no cache at all.'"

---

## Q7: شو الفرق بين AuditLoggingMiddleware وAction Filter؟

**الجواب:**
"Action Filter بيشتغل بس لما الـ request يوصل لـ Controller.
لو Nurse طلبت Doctor-only endpoint، الـ 403 بيرجع قبل ما يوصل للـ Controller —
Action Filter ما بيشوفه.

Middleware بيشتغل على كل request بدون استثناء، حتى الـ 401 والـ 429.
بالنظم الطبية هاد requirement: الـ rejected requests هي بالضبط اللي
بدك تتبّعها — مش بس الناجحة.

لهيك وضعته بعد UseAuthentication وبعد _next: عشان claims تكون محلولة
والـ status code يكون محدد قبل ما أكتب الـ log."
