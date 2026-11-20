/* Test page logic: calls the Employee API and shows exactly what it answered. */
(function () {
  'use strict';

  const API = '/api/employees';

  const rows = document.getElementById('employee-rows');
  const emptyState = document.getElementById('empty-state');
  const count = document.getElementById('employee-count');
  const form = document.getElementById('employee-form');
  const lookupForm = document.getElementById('lookup-form');
  const log = document.getElementById('log');
  const banner = document.getElementById('banner');

  /** Sends a request and returns { ok, status, body }. Never throws. */
  async function callApi(method, url, payload) {
    const options = { method: method, headers: { Accept: 'application/json' } };
    if (payload !== undefined) {
      options.headers['Content-Type'] = 'application/json';
      options.body = JSON.stringify(payload);
    }

    let result;
    try {
      const response = await fetch(url, options);
      const text = await response.text();
      let body = null;
      if (text) {
        try { body = JSON.parse(text); } catch (parseError) { body = text; }
      }
      result = { ok: response.ok, status: response.status, body: body };
    } catch (error) {
      result = { ok: false, status: 0, body: { title: 'Cannot reach the API', detail: error.message } };
    }

    writeLog(method, url, result);
    return result;
  }

  /** Turns a ProblemDetails / validation error body into one readable line. */
  function describeError(result) {
    const body = result.body;
    if (!body || typeof body !== 'object') {
      return 'Request failed (HTTP ' + result.status + ').';
    }
    if (body.errors) {
      return Object.keys(body.errors)
        .map(function (field) { return field + ': ' + body.errors[field].join(' '); })
        .join(' | ');
    }
    return [body.title, body.detail].filter(Boolean).join(': ') || 'Request failed (HTTP ' + result.status + ').';
  }

  function showBanner(message, kind) {
    banner.textContent = message;
    banner.className = 'banner ' + kind;
    banner.hidden = false;
  }

  function writeLog(method, url, result) {
    const entry = document.createElement('div');
    entry.className = 'log-entry ' + (result.ok ? 'ok' : 'error');

    const heading = document.createElement('div');
    heading.className = 'log-heading';
    heading.textContent =
      new Date().toLocaleTimeString() + '  ' + method + ' ' + url + '  ->  ' + (result.status || 'no response');

    const body = document.createElement('pre');
    body.textContent = result.body === null ? '(empty body)' : JSON.stringify(result.body, null, 2);

    entry.appendChild(heading);
    entry.appendChild(body);
    log.prepend(entry);
  }

  function cell(text) {
    const td = document.createElement('td');
    td.textContent = text;
    return td;
  }

  function renderEmployees(employees) {
    rows.replaceChildren();
    employees.forEach(function (employee) {
      const tr = document.createElement('tr');
      tr.appendChild(cell(employee.id));
      tr.appendChild(cell(employee.firstName + ' ' + employee.lastName));
      tr.appendChild(cell(employee.email));
      tr.appendChild(cell(employee.department));
      tr.appendChild(cell(new Date(employee.createdAtUtc).toLocaleString()));

      const actions = document.createElement('td');
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'danger';
      button.textContent = 'Delete';
      button.addEventListener('click', function () { deleteEmployee(employee.id); });
      actions.appendChild(button);
      tr.appendChild(actions);

      rows.appendChild(tr);
    });

    emptyState.hidden = employees.length > 0;
    count.textContent = employees.length;
  }

  async function refresh() {
    const result = await callApi('GET', API);
    if (result.ok) {
      renderEmployees(result.body);
    } else {
      showBanner(describeError(result), 'error');
    }
  }

  async function deleteEmployee(id) {
    const result = await callApi('DELETE', API + '/' + encodeURIComponent(id));
    if (result.ok) {
      showBanner('Deleted employee ' + id + '.', 'ok');
    } else {
      showBanner(describeError(result), 'error');
    }
    await refresh();
  }

  form.addEventListener('submit', async function (event) {
    event.preventDefault();
    const payload = Object.fromEntries(new FormData(form).entries());

    const result = await callApi('POST', API, payload);
    if (result.ok) {
      showBanner('Created employee ' + result.body.id + '.', 'ok');
      form.reset();
      await refresh();
    } else {
      showBanner(describeError(result), 'error');
    }
  });

  lookupForm.addEventListener('submit', async function (event) {
    event.preventDefault();
    const id = document.getElementById('lookup-id').value.trim();
    const action = event.submitter && event.submitter.value;

    if (action === 'delete') {
      await deleteEmployee(id);
      return;
    }

    const result = await callApi('GET', API + '/' + encodeURIComponent(id));
    if (result.ok) {
      showBanner('Found employee ' + result.body.id + ': ' +
        result.body.firstName + ' ' + result.body.lastName + ' (' + result.body.email + ').', 'ok');
    } else {
      showBanner(describeError(result), 'error');
    }
  });

  document.getElementById('refresh').addEventListener('click', refresh);

  document.getElementById('load-sample').addEventListener('click', async function () {
    const summary = await window.loadSampleData();
    const message = summary.created.length + ' created, ' + summary.skipped.length +
      ' skipped (already exist), ' + summary.failed.length + ' failed.';
    showBanner('Sample data: ' + message, summary.failed.length > 0 ? 'error' : 'ok');
    await refresh();
  });

  document.getElementById('clear-log').addEventListener('click', function () {
    log.replaceChildren();
  });

  refresh();
})();
