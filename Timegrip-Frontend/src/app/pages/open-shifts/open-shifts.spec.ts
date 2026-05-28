import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OpenShifts } from './open-shifts';

describe('OpenShifts', () => {
  let component: OpenShifts;
  let fixture: ComponentFixture<OpenShifts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [OpenShifts],
    }).compileComponents();

    fixture = TestBed.createComponent(OpenShifts);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
