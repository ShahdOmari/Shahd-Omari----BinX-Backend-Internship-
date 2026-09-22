# Rehearsal Checklist

## Timing Target
- Slides: 7-8 minutes
- Live Demo: 8-10 minutes
- Q&A: 5-7 minutes
- Total: 20-25 minutes

## Before Rehearsal
- [ ] Railway API responding: GET /health → 200
- [ ] Postman: Admin token ready
- [ ] Postman: Nurse token ready
- [ ] GitHub Actions tab open — green pipeline visible
- [ ] jwt.io tab open
- [ ] Slides open on correct first slide

## During Demo — Things to Say Out Loud
- [ ] Explain WHY 403 not 401 when showing Nurse rejection
- [ ] Explain WHY staffProfileId is in the JWT (not just that it is)
- [ ] Show the response TIME in Postman — point to it explicitly
- [ ] Say "this number came from the terminal, not a benchmark tool"
- [ ] When showing cache hit: "watch the time drop"

## After Rehearsal — Self-Check
- [ ] Did demo finish under 10 minutes?
- [ ] Did I explain decisions, not just features?
- [ ] Did I cite specific numbers (2651ms, 3ms, 300 rows, 7 rows, 34 tests)?
- [ ] Do I know the answer to all 7 Q&A questions?
- [ ] Is the backup plan (localhost) ready if Railway has issues?

## Common Mistakes to Avoid
- Saying "I added caching for performance" without the numbers
- Skipping the ownership check scene (most unique security feature)
- Reading from slides instead of explaining
- Forgetting to show the CI pipeline green checkmarks
