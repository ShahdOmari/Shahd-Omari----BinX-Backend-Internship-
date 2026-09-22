# Full Demo Script — Cardiac Patient Monitoring System
## Presentation Day Demo — Timed Rehearsal Version

**Total demo time: 8-10 minutes**
**Tools open before starting:** Postman, browser (Railway URL), GitHub

---

## Pre-Demo Checklist (do this 5 minutes before)
- [ ] Railway API is responding: GET https://[your-url]/health → 200
- [ ] Postman collection open, Cardiac-Local environment active
- [ ] GitHub repo open on the README tab
- [ ] Terminal ready (if showing Swagger locally)
- [ ] Browser tab open on GitHub Actions (to show green pipeline)

---

## Scene 1 — The Live API (1 min)

**Open browser → Railway URL → /health**

"هاي الـ API شغالة live على Railway. كل مرة بعمل push على main،
GitHub Actions بيبني، بيشغل الـ 34 اختبار، ولو كلهم نجحوا بنشر
على الـ Railway تلقائياً."

**Open GitHub → Actions tab → show green checkmarks**

"هاد الـ pipeline — build، test، deploy. لو اختبار واحد فشل،
الـ deploy ما بيصير."

---

## Scene 2 — Authentication & JWT (1.5 min)

**Postman → POST /auth/login (admin@cardiac.com)**

"بسجّل دخول بحساب الـ Admin وبآخذ JWT token."

**Open jwt.io → paste token**

"خلينا نحلّل التوكن. شايفين ثلاث claims مهمين:
- `sub` — هوية المستخدم
- `role: Admin` — الدور
- `staffProfileId` — هاد الـ claim المميز"

"ليش staffProfileId مهم؟ لأنه بكل request بعدها، الـ API
بتعرف مين الموظف بدون ما تسأل قاعدة البيانات مرة ثانية."

---

## Scene 3 — RBAC في العمل (2 min)

**Postman → Register new Nurse account**

"سجّلت حساب جديد — تلقائياً بياخذ دور Nurse."

**Postman → GET /VitalSigns/critical with Nurse token**

"ممرضة مصادق عليها بتحاول تشوف قائمة المرضى الحرجين."

**Show 403 Forbidden**

"403 — مش 401. الفرق مهم:
- 401: ما بعرف مين أنت
- 403: بعرف مين أنت، والجواب لا

هاد endpoint مقصور على Doctor وAdmin لأنه بيظهر المرضى
اللي بحاجة تدخل فوري."

**Postman → GET /VitalSigns/critical with Admin token**

"نفس الـ endpoint بتوكن Admin → 200، 7 نتائج."

---

## Scene 4 — Ownership Check (1.5 min)

**Postman → POST /VitalSigns with Nurse A token → note the ID**

"Nurse A سجّلت قراءة حيوية."

**Postman → GET /VitalSigns/{id} with Nurse B token**

"Nurse B — نفس الدور بالضبط — بتحاول تقرأ نفس السجل."

**Show 403**

"403. الـ RBAC وحده ما يكفي — محتاجين ownership check.
الـ API بتقارن الـ staffProfileId بالتوكن مع الـ RecordedByStaffProfileId
بالسجل. نفس الدور، صلاحيات مختلفة."

---

## Scene 5 — Performance: Redis Cache (2 min)

**Postman → GET /Patients (first time)**

"أول request."

**Show response time in Postman**

**Postman → GET /Patients (second time immediately)**

"ثاني request — نفس الـ endpoint."

**Compare the two times**

"الفرق هاد هو Redis. أول مرة: query لقاعدة البيانات وكتابة للـ cache.
ثاني مرة: مباشرة من الـ cache بدون ما تلمس الـ DB."

"الأرقام من مشروعي: 2651ms → 3ms. 98% أسرع."

"والـ invalidation شغال — لو أضفت مريض جديد، المرة الجاية بتشوفه فوراً."

---

## Scene 6 — Performance: Query Optimization (1 min)

**Show the code side-by-side (or slide)**
BEFORE: AFTER:
SELECT * FROM VitalSigns WHERE RiskLevel = 'Critical'
-- 300 rows to RAM AND RecordedAtUtc = (SELECT MAX...)
-- C# filters in memory -- 7 rows only

"هاد الفرق بين فلترة بالـ RAM وفلترة بالـ SQL.
مع 300 سجل الفرق يبدو صغير.
مع 300,000 سجل — مستشفى حقيقي — الأول بيسقط السيرفر."

---

## Closing (30 sec)

"خلاصة:
- أمان بـ 4 طبقات
- أداء موثّق بأرقام حقيقية
- 34 اختبار، CI/CD، deployment تلقائي

شكراً."

---

## Backup Demo Plan (لو الـ Railway انقطع)

1. شغلي `dotnet run` محلياً
2. استخدمي `http://localhost:5286/swagger`
3. نفس الـ demo بالـ Swagger بدل Postman
