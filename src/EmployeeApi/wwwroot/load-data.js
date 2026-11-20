/*
 * Loads sample employees into the Employee API.
 *
 * Works three ways:
 *   1. Test page:        click "Load sample data" at http://localhost:5080/
 *   2. Node 18+:         node load-data.js [baseUrl]      (default http://localhost:5080)
 *   3. Browser console:  paste this file, then run:  await loadSampleData()
 *
 * Employees whose email already exists (409) are skipped, so it is safe to re-run.
 */
(function () {
  'use strict';

  const DEFAULT_BASE_URL = 'http://localhost:5080';

  const SAMPLE_EMPLOYEES = [
    { firstName: 'Ada', lastName: 'Lovelace', email: 'ada.lovelace@example.com', department: 'Engineering' },
    { firstName: 'Grace', lastName: 'Hopper', email: 'grace.hopper@example.com', department: 'Engineering' },
    { firstName: 'Alan', lastName: 'Turing', email: 'alan.turing@example.com', department: 'Research' },
    { firstName: 'Katherine', lastName: 'Johnson', email: 'katherine.johnson@example.com', department: 'Research' },
    { firstName: 'Linus', lastName: 'Torvalds', email: 'linus.torvalds@example.com', department: 'Platform' },
    { firstName: 'Margaret', lastName: 'Hamilton', email: 'margaret.hamilton@example.com', department: 'Platform' },
    { firstName: 'Dennis', lastName: 'Ritchie', email: 'dennis.ritchie@example.com', department: 'Engineering' },
    { firstName: 'Barbara', lastName: 'Liskov', email: 'barbara.liskov@example.com', department: 'Architecture' },
    { firstName: 'Tim', lastName: 'Berners-Lee', email: 'tim.berners-lee@example.com', department: 'Web' },
    { firstName: 'Radia', lastName: 'Perlman', email: 'radia.perlman@example.com', department: 'Networking' },
  ];

  function defaultBaseUrl() {
    const inBrowserOnHttp = typeof location !== 'undefined' && location.protocol.startsWith('http');
    return inBrowserOnHttp ? location.origin : DEFAULT_BASE_URL;
  }

  /**
   * Posts each sample employee in order (so ids are assigned in list order).
   * @param {string} [baseUrl] API root, e.g. http://localhost:5080
   * @returns {Promise<{created: object[], skipped: string[], failed: {email: string, reason: string}[]}>}
   */
  async function loadSampleData(baseUrl) {
    const url = (baseUrl || defaultBaseUrl()).replace(/\/+$/, '') + '/api/employees';
    const summary = { created: [], skipped: [], failed: [] };

    for (const employee of SAMPLE_EMPLOYEES) {
      try {
        const response = await fetch(url, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(employee),
        });

        if (response.status === 201) {
          summary.created.push(await response.json());
        } else if (response.status === 409) {
          summary.skipped.push(employee.email);
        } else {
          const problem = await response.json().catch(() => null);
          const reason = (problem && (problem.detail || problem.title)) || 'HTTP ' + response.status;
          summary.failed.push({ email: employee.email, reason: reason });
        }
      } catch (error) {
        // Network-level failure (API not running, wrong URL, CORS).
        summary.failed.push({ email: employee.email, reason: error.message });
      }
    }

    console.log(
      'Sample data: ' + summary.created.length + ' created, ' +
      summary.skipped.length + ' skipped (already exist), ' +
      summary.failed.length + ' failed.');
    summary.failed.forEach(function (failure) {
      console.error('  ' + failure.email + ': ' + failure.reason);
    });

    return summary;
  }

  if (typeof window !== 'undefined') {
    window.loadSampleData = loadSampleData;
  }

  const runFromNode = typeof module !== 'undefined' && typeof require !== 'undefined' && require.main === module;
  if (runFromNode) {
    loadSampleData(process.argv[2]).then(function (summary) {
      process.exitCode = summary.failed.length > 0 ? 1 : 0;
    });
  }
})();
