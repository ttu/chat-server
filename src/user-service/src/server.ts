import Koa from 'koa';
import Router from 'koa-router';

const port = 5590;
const app = new Koa();

app.use(async (ctx: Koa.Context, next: () => void) => {
    console.log('Url:', ctx.url);
    await next();
});

const router = new Router();

router.get('/', async (ctx: Koa.Context) => {
    ctx.body = 'Hello from User Service!';
});

router.get('/online', async (ctx: Koa.Context) => {
    ctx.body = ['JJ', 'AA'];
});


app.use(router.routes());

app.listen(port);

console.log(`Server running on port ${port}`);