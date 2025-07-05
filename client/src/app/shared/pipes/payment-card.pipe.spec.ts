import { PaymentCardPipe } from './payment-card.pipe';

describe('PaymentCardPipe', () => {
  it('create an instance', () => {
    const pipe = new PaymentCardPipe();
    expect(pipe).toBeTruthy();
  });
});
