(() => {
  const form = document.querySelector('#login-form');
  const password = document.querySelector('#password');
  const status = document.querySelector('#login-status');

  async function csrfToken() {
    const response = await fetch('/api/admin/antiforgery', { cache: 'no-store' });
    if (!response.ok) throw new Error('Could not prepare a secure sign-in request.');
    return (await response.json()).token;
  }

  form.addEventListener('submit', async (event) => {
    event.preventDefault();
    status.classList.remove('error');
    status.textContent = 'Signing in…';

    try {
      const token = await csrfToken();
      const response = await fetch('/api/admin/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': token },
        body: JSON.stringify({ password: password.value })
      });
      if (response.ok) {
        window.location.assign('/admin/');
        return;
      }
      const problem = await response.json().catch(() => ({}));
      throw new Error(problem.detail || (response.status === 401 ? 'The password was not accepted.' : 'Sign-in is unavailable.'));
    } catch (error) {
      status.classList.add('error');
      status.textContent = error.message;
    }
  });
})();
